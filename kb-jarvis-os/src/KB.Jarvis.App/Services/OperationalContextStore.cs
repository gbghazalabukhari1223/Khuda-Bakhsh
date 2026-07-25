using System.Text.Json;
using System.Text.Json.Nodes;

namespace KB.Jarvis.App.Services;

public sealed record OperationalContextEvent(
    DateTimeOffset At,
    string Kind,
    string Summary,
    string? Evidence = null);

public sealed class OperationalMissionState
{
    public string MissionId { get; set; } = Guid.NewGuid().ToString("N");
    public string Source { get; set; } = "system";
    public string Objective { get; set; } = string.Empty;
    public string Status { get; set; } = "READY";
    public string CurrentStep { get; set; } = string.Empty;
    public string ActiveApplication { get; set; } = string.Empty;
    public string ActiveWindow { get; set; } = string.Empty;
    public string BrowserDomain { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string ExpectedPostcondition { get; set; } = string.Empty;
    public string LastVerifiedEvidence { get; set; } = string.Empty;
    public string RecoveryCheckpoint { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public List<OperationalContextEvent> Events { get; set; } = new();
}

public static class OperationalContextStore
{
    private static readonly object Gate = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static OperationalMissionState _state = LoadInternal();

    private static string DirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KB Jarvis OS",
        "OperationalContext");

    private static string StatePath => Path.Combine(DirectoryPath, "current-mission.json");

    public static string BeginMission(string source, string objective)
    {
        var cleanObjective = Limit(objective, 2400);
        lock (Gate)
        {
            _state = new OperationalMissionState
            {
                MissionId = Guid.NewGuid().ToString("N"),
                Source = string.IsNullOrWhiteSpace(source) ? "unknown" : source.Trim(),
                Objective = cleanObjective,
                Status = "PLANNING",
                CurrentStep = "Understand the objective and inspect current state",
                ExpectedPostcondition = "The requested outcome is independently verified",
                UpdatedAt = DateTimeOffset.UtcNow,
                Events = new List<OperationalContextEvent>
                {
                    new(DateTimeOffset.UtcNow, "MISSION_STARTED", cleanObjective)
                }
            };
            SaveInternal();
            return _state.MissionId;
        }
    }

    public static void RecordTool(string functionName, JsonElement args, string result)
    {
        lock (Gate)
        {
            _state.Status = ResultStatus(result);
            _state.CurrentStep = $"Tool completed: {functionName}";
            _state.UpdatedAt = DateTimeOffset.UtcNow;
            _state.Events.Add(new OperationalContextEvent(
                DateTimeOffset.UtcNow,
                "TOOL_RESULT",
                functionName,
                $"args={Limit(Sanitize(args).ToJsonString(), 1600)}; result={Limit(result, 2600)}"));
            if (result.Contains("verified", StringComparison.OrdinalIgnoreCase)
                || result.StartsWith("Completed:", StringComparison.OrdinalIgnoreCase)
                || result.StartsWith("VERIFIED", StringComparison.OrdinalIgnoreCase))
            {
                _state.LastVerifiedEvidence = Limit(result, 1800);
                _state.RecoveryCheckpoint = $"After verified tool {functionName}";
            }
            TrimEvents();
            SaveInternal();
        }
    }

    public static void RecordAssistant(string text)
    {
        lock (Gate)
        {
            _state.UpdatedAt = DateTimeOffset.UtcNow;
            _state.Events.Add(new OperationalContextEvent(
                DateTimeOffset.UtcNow,
                "ASSISTANT_RESPONSE",
                Limit(text, 2600)));
            TrimEvents();
            SaveInternal();
        }
    }

    public static void RecordVoiceInput(string text)
    {
        lock (Gate)
        {
            if (string.IsNullOrWhiteSpace(_state.Objective))
            {
                BeginMission("voice", text);
                return;
            }
            _state.UpdatedAt = DateTimeOffset.UtcNow;
            _state.Events.Add(new OperationalContextEvent(
                DateTimeOffset.UtcNow,
                "VOICE_INPUT",
                Limit(text, 1800)));
            TrimEvents();
            SaveInternal();
        }
    }

    public static void RecordState(
        string status,
        string currentStep,
        string? target = null,
        string? evidence = null,
        string? expectedPostcondition = null)
    {
        lock (Gate)
        {
            _state.Status = string.IsNullOrWhiteSpace(status) ? _state.Status : status.Trim();
            _state.CurrentStep = Limit(currentStep, 1200);
            if (!string.IsNullOrWhiteSpace(target)) _state.Target = Limit(target, 1200);
            if (!string.IsNullOrWhiteSpace(evidence)) _state.LastVerifiedEvidence = Limit(evidence, 1800);
            if (!string.IsNullOrWhiteSpace(expectedPostcondition))
                _state.ExpectedPostcondition = Limit(expectedPostcondition, 1200);
            _state.UpdatedAt = DateTimeOffset.UtcNow;
            _state.Events.Add(new OperationalContextEvent(
                DateTimeOffset.UtcNow,
                "STATE",
                $"{_state.Status}: {_state.CurrentStep}",
                evidence is null ? null : Limit(evidence, 1600)));
            TrimEvents();
            SaveInternal();
        }
    }

    public static string CreateModelContext()
    {
        lock (Gate)
        {
            var recent = _state.Events
                .TakeLast(10)
                .Select(item => $"- {item.At:O} [{item.Kind}] {item.Summary}" +
                                (string.IsNullOrWhiteSpace(item.Evidence) ? string.Empty : $" | {item.Evidence}"));
            return Limit($"""
Mission ID: {_state.MissionId}
Source: {_state.Source}
Objective: {_state.Objective}
Status: {_state.Status}
Current step: {_state.CurrentStep}
Active application: {_state.ActiveApplication}
Active window: {_state.ActiveWindow}
Browser domain: {_state.BrowserDomain}
Target: {_state.Target}
Expected postcondition: {_state.ExpectedPostcondition}
Last verified evidence: {_state.LastVerifiedEvidence}
Recovery checkpoint: {_state.RecoveryCheckpoint}
Updated: {_state.UpdatedAt:O}
Recent mission events:
{string.Join(Environment.NewLine, recent)}
""", 7200);
        }
    }

    private static OperationalMissionState LoadInternal()
    {
        try
        {
            if (!File.Exists(StatePath)) return new OperationalMissionState();
            return JsonSerializer.Deserialize<OperationalMissionState>(File.ReadAllText(StatePath), JsonOptions)
                   ?? new OperationalMissionState();
        }
        catch
        {
            return new OperationalMissionState();
        }
    }

    private static void SaveInternal()
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var temporary = StatePath + ".tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(_state, JsonOptions));
            File.Move(temporary, StatePath, overwrite: true);
        }
        catch
        {
            // Operational context must never crash the main assistant.
        }
    }

    private static JsonNode Sanitize(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => SanitizeObject(element),
            JsonValueKind.Array => new JsonArray(element.EnumerateArray().Select(Sanitize).ToArray()),
            JsonValueKind.String => JsonValue.Create(Limit(element.GetString() ?? string.Empty, 1200))!,
            JsonValueKind.Number => JsonNode.Parse(element.GetRawText())!,
            JsonValueKind.True => JsonValue.Create(true)!,
            JsonValueKind.False => JsonValue.Create(false)!,
            _ => JsonValue.Create(string.Empty)!
        };
    }

    private static JsonObject SanitizeObject(JsonElement element)
    {
        var result = new JsonObject();
        foreach (var property in element.EnumerateObject())
        {
            var sensitive = property.Name.Contains("key", StringComparison.OrdinalIgnoreCase)
                            || property.Name.Contains("token", StringComparison.OrdinalIgnoreCase)
                            || property.Name.Contains("password", StringComparison.OrdinalIgnoreCase)
                            || property.Name.Contains("secret", StringComparison.OrdinalIgnoreCase);
            result[property.Name] = sensitive ? "[REDACTED]" : Sanitize(property.Value);
        }
        return result;
    }

    private static string ResultStatus(string result)
    {
        if (result.Contains("confirmation required", StringComparison.OrdinalIgnoreCase)) return "PREPARED";
        if (result.StartsWith("Blocked", StringComparison.OrdinalIgnoreCase)) return "BLOCKED";
        if (result.StartsWith("Failed", StringComparison.OrdinalIgnoreCase)
            || result.Contains("Tool failed", StringComparison.OrdinalIgnoreCase)) return "FAILED";
        if (result.StartsWith("Completed", StringComparison.OrdinalIgnoreCase)
            || result.Contains("verified", StringComparison.OrdinalIgnoreCase)) return "VERIFIED";
        return "PARTIAL";
    }

    private static void TrimEvents()
    {
        if (_state.Events.Count > 40)
        {
            _state.Events = _state.Events.TakeLast(40).ToList();
        }
    }

    private static string Limit(string value, int length) =>
        string.IsNullOrEmpty(value) || value.Length <= length ? value : value[..length] + "…";
}
