using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KB.Jarvis.App.Services;

public sealed class GeminiTextAgentService
{
    private readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(90) };

    public async Task<string> SendAsync(
        string prompt,
        JarvisSettings settings,
        Func<string, JsonElement, CancellationToken, Task<string>> toolExecutor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured. Open Settings and save the key first.");
        }

        var initialParts = new JsonArray(new JsonObject { ["text"] = prompt });
        AppendParts(initialParts, VisualContextHub.CreateInlineParts("Visual context before the task"));
        var contents = new JsonArray
        {
            new JsonObject { ["role"] = "user", ["parts"] = initialParts }
        };

        for (var round = 0; round < 8; round++)
        {
            var response = await GenerateAsync(contents, settings, cancellationToken).ConfigureAwait(false);
            var content = response?["candidates"]?[0]?["content"] as JsonObject
                ?? throw new InvalidOperationException(ReadApiError(response) ?? "Gemini returned no candidate content.");
            var parts = content["parts"] as JsonArray ?? new JsonArray();
            var text = string.Join("", parts
                .OfType<JsonObject>()
                .Select(part => part["text"]?.GetValue<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value)));

            var calls = parts
                .OfType<JsonObject>()
                .Where(part => part["functionCall"] is JsonObject)
                .ToList();
            if (calls.Count == 0)
            {
                return string.IsNullOrWhiteSpace(text)
                    ? "Boss, the model returned an empty response."
                    : text.Trim();
            }

            contents.Add(content.DeepClone());
            var functionParts = new JsonArray();
            foreach (var callPart in calls)
            {
                var functionCall = (JsonObject)callPart["functionCall"]!;
                var name = functionCall["name"]?.GetValue<string>()
                    ?? throw new InvalidOperationException("Gemini requested an unnamed function.");
                var argsNode = functionCall["args"] ?? new JsonObject();
                using var argsDocument = JsonDocument.Parse(argsNode.ToJsonString());
                string result;
                try
                {
                    result = await toolExecutor(name, argsDocument.RootElement.Clone(), cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    result = $"Tool failed: {exception.Message}";
                }

                functionParts.Add(new JsonObject
                {
                    ["functionResponse"] = new JsonObject
                    {
                        ["name"] = name,
                        ["response"] = new JsonObject { ["result"] = result }
                    }
                });
            }

            AppendParts(functionParts, VisualContextHub.CreateInlineParts("Updated visual context after the local action"));
            contents.Add(new JsonObject { ["role"] = "user", ["parts"] = functionParts });
        }

        throw new InvalidOperationException("Gemini exceeded the maximum local tool-call rounds for one request.");
    }

    private async Task<JsonNode?> GenerateAsync(
        JsonArray contents,
        JarvisSettings settings,
        CancellationToken cancellationToken)
    {
        var model = settings.TextModel.Trim().Replace("models/", string.Empty, StringComparison.OrdinalIgnoreCase);
        var uri = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(settings.ApiKey)}";
        var body = new JsonObject
        {
            ["systemInstruction"] = new JsonObject
            {
                ["parts"] = new JsonArray(new JsonObject { ["text"] = GeminiToolCatalog.SystemInstruction })
            },
            ["contents"] = contents.DeepClone(),
            ["tools"] = new JsonArray(JsonSerializer.SerializeToNode(new
            {
                functionDeclarations = GeminiToolCatalog.CreateFunctionDeclarations()
            })!),
            ["generationConfig"] = new JsonObject
            {
                ["temperature"] = 0.2,
                ["maxOutputTokens"] = 1800
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(uri, body, cancellationToken).ConfigureAwait(false);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var node = JsonNode.Parse(raw);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(ReadApiError(node) ?? $"Gemini request failed with HTTP {(int)response.StatusCode}.");
        }
        return node;
    }

    private static void AppendParts(JsonArray target, JsonArray source)
    {
        foreach (var item in source)
        {
            target.Add(item?.DeepClone());
        }
    }

    private static string? ReadApiError(JsonNode? node) =>
        node?["error"]?["message"]?.GetValue<string>();
}