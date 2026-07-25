using System.Text.Json;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App.Skills;

public sealed class YouTubeSkill : IJarvisSkill
{
    private readonly BrowserBridgeService _bridge;

    public YouTubeSkill(BrowserBridgeService bridge)
    {
        _bridge = bridge;
    }

    public string Id => "browser.youtube.play";
    public string DisplayName => "YouTube — Search, Open and Verify Playback";

    public bool CanHandle(SkillRequest request)
    {
        if (request.Arguments.ContainsKey("query")) return true;
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("youtube")
               && (goal.Contains("play") || goal.Contains("song") || goal.Contains("video") || goal.Contains("chala") || goal.Contains("لگاؤ"));
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        if (!_bridge.IsConnected)
        {
            return new SkillResult(Id, SkillStatus.Blocked, "The Browser Companion is not connected, so YouTube playback cannot be verified.", Array.Empty<SkillStepResult>());
        }

        var query = request.Arguments.TryGetValue("query", out var supplied) ? supplied.Trim() : ExtractQuery(request.Goal);
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SkillResult(Id, SkillStatus.Blocked, "A YouTube song or video name is required.", Array.Empty<SkillStepResult>());
        }

        var result = await _bridge.ExecuteAsync(
            "youtube.play",
            new { query },
            TimeSpan.FromSeconds(25),
            request.CancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new SkillResult(
                Id,
                SkillStatus.Failed,
                result.Error ?? "YouTube playback could not be verified.",
                new[] { new SkillStepResult("Search and play YouTube result", false, result.Error ?? "No verified playback result.") });
        }

        var title = Read(result.Data, "title") ?? query;
        var playing = ReadBool(result.Data, "playing");
        var url = Read(result.Data, "url");
        return new SkillResult(
            Id,
            playing ? SkillStatus.Completed : SkillStatus.Prepared,
            playing
                ? $"YouTube is playing: {title}"
                : $"YouTube opened the selected result for {query}, but audible playback could not be fully verified.",
            new[]
            {
                new SkillStepResult("Open YouTube search", true, query),
                new SkillStepResult("Select best video", !string.IsNullOrWhiteSpace(url), url ?? title),
                new SkillStepResult("Verify media playback", playing, playing ? $"Playing: {title}" : "Video page opened; playback state remained unverified.")
            });
    }

    private static string ExtractQuery(string goal)
    {
        var remove = new[] { "youtube", "open", "play", "song", "video", "please", "on", "and", "chalao", "chala", "laga", "do", "karo", "kro" };
        var result = goal;
        foreach (var word in remove)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result, $@"\b{System.Text.RegularExpressions.Regex.Escape(word)}\b", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return string.Join(' ', result.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string? Read(JsonElement? data, string name) =>
        data is { } element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;

    private static bool ReadBool(JsonElement? data, string name) =>
        data is { } element && element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
}