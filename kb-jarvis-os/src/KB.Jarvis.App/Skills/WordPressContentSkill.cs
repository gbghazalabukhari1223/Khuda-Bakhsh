using System.Text.Json;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App.Skills;

public sealed class WordPressContentSkill : IJarvisSkill
{
    private readonly BrowserBridgeService _bridge;

    public WordPressContentSkill(BrowserBridgeService bridge)
    {
        _bridge = bridge;
    }

    public string Id => "browser.wordpress.content";
    public string DisplayName => "WordPress — Create and Edit Pages or Posts in Existing Admin Session";

    public bool CanHandle(SkillRequest request)
    {
        if (request.Arguments.ContainsKey("action")) return true;
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("wordpress")
               || goal.Contains("create post")
               || goal.Contains("create page")
               || goal.Contains("page banao")
               || goal.Contains("post banao")
               || goal.Contains("wp-admin");
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        if (!_bridge.IsConnected)
        {
            return new SkillResult(Id, SkillStatus.Blocked, "The Browser Companion is not connected. WordPress must use an existing signed-in wp-admin tab.", Array.Empty<SkillStepResult>());
        }

        var action = Get(request, "action", "create_post").ToLowerInvariant();
        var status = Get(request, "status", "draft").ToLowerInvariant();
        var title = Get(request, "title", string.Empty);
        var content = Get(request, "content", string.Empty);
        var source = Get(request, "source", string.Empty).ToLowerInvariant();
        var confirmed = GetBool(request, "confirmed", false);

        if (string.IsNullOrWhiteSpace(content) && source == "chatgpt")
        {
            var latest = await _bridge.ExecuteAsync(
                "chatgpt.latest_response",
                new { },
                TimeSpan.FromSeconds(10),
                request.CancellationToken).ConfigureAwait(false);
            if (!latest.Success)
            {
                return new SkillResult(Id, SkillStatus.Blocked, latest.Error ?? "The latest ChatGPT response could not be read.", Array.Empty<SkillStepResult>());
            }
            content = Read(latest.Data, "text") ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(title) && action is not "update_current")
        {
            return new SkillResult(Id, SkillStatus.Blocked, "A WordPress page or post title is required.", Array.Empty<SkillStepResult>());
        }
        if (string.IsNullOrWhiteSpace(content))
        {
            return new SkillResult(Id, SkillStatus.Blocked, "WordPress content is required, or set source to chatgpt while a ChatGPT response is open.", Array.Empty<SkillStepResult>());
        }

        if (status == "publish" && !confirmed)
        {
            return new SkillResult(
                Id,
                SkillStatus.Prepared,
                $"WordPress content is ready to publish: {title}",
                new[] { new SkillStepResult("Prepare WordPress content", true, $"{content.Length} characters") },
                RequiresConfirmation: true,
                ConfirmationPrompt: $"Publish this WordPress {ReadableType(action)} now?\n\n{title}");
        }

        var result = await _bridge.ExecuteAsync(
            "wordpress.content",
            new { action, title, content, status },
            TimeSpan.FromSeconds(35),
            request.CancellationToken).ConfigureAwait(false);
        if (!result.Success)
        {
            return new SkillResult(
                Id,
                SkillStatus.Failed,
                result.Error ?? "WordPress content operation failed.",
                new[] { new SkillStepResult("Operate existing WordPress editor", false, result.Error ?? "No verified result.") });
        }

        var editorTitle = Read(result.Data, "title") ?? title;
        var saved = ReadBool(result.Data, "saved");
        var published = ReadBool(result.Data, "published");
        var url = Read(result.Data, "url");
        var completed = status == "publish" ? published : saved;
        return new SkillResult(
            Id,
            completed ? SkillStatus.Completed : SkillStatus.Prepared,
            completed
                ? status == "publish" ? $"WordPress content published and verified: {editorTitle}" : $"WordPress draft saved and verified: {editorTitle}"
                : $"WordPress editor was filled for {editorTitle}, but final save state could not be fully verified.",
            new[]
            {
                new SkillStepResult("Use existing wp-admin tab", true, url ?? "Existing WordPress admin session"),
                new SkillStepResult("Fill title and content", true, $"{content.Length} characters"),
                new SkillStepResult(status == "publish" ? "Publish and verify" : "Save draft and verify", completed, editorTitle)
            });
    }

    private static string ReadableType(string action) => action.Contains("page", StringComparison.OrdinalIgnoreCase) ? "page" : "post";
    private static string Get(SkillRequest request, string key, string fallback) => request.Arguments.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    private static bool GetBool(SkillRequest request, string key, bool fallback) => request.Arguments.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;
    private static string? Read(JsonElement? data, string name) => data is { } element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString() : null;
    private static bool ReadBool(JsonElement? data, string name) => data is { } element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
}