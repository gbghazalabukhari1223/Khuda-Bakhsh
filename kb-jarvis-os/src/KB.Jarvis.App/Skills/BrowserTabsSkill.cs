using System.Text.Json;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App.Skills;

public sealed class BrowserTabsSkill : IJarvisSkill
{
    private readonly BrowserBridgeService _bridge;

    public BrowserTabsSkill(BrowserBridgeService bridge)
    {
        _bridge = bridge;
    }

    public string Id => "browser.tabs.list";
    public string DisplayName => "Browser — List Active Tabs";

    public bool CanHandle(SkillRequest request)
    {
        var goal = request.Goal.ToLowerInvariant();
        return (goal.Contains("tab") || goal.Contains("browser"))
               && (goal.Contains("list")
                   || goal.Contains("show")
                   || goal.Contains("active")
                   || goal.Contains("which")
                   || goal.Contains("kon se")
                   || goal.Contains("konsi"));
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        if (!_bridge.IsConnected)
        {
            return new SkillResult(
                Id,
                SkillStatus.Blocked,
                "The Browser Companion is not connected, so browser tabs cannot be inspected.",
                Array.Empty<SkillStepResult>());
        }

        var result = await _bridge.ExecuteAsync(
            "tabs.list",
            new { },
            TimeSpan.FromSeconds(8),
            request.CancellationToken).ConfigureAwait(false);

        if (!result.Success || result.Data is not { } data || data.ValueKind != JsonValueKind.Array)
        {
            return new SkillResult(
                Id,
                SkillStatus.Failed,
                result.Error ?? "The Browser Companion returned no tab list.",
                new[] { new SkillStepResult("Read browser tabs", false, result.Error ?? "No tab data returned.") });
        }

        var tabs = data.EnumerateArray()
            .Select(tab => new
            {
                Title = tab.TryGetProperty("title", out var title) ? title.GetString() : null,
                Url = tab.TryGetProperty("url", out var url) ? url.GetString() : null,
                Active = tab.TryGetProperty("active", out var active) && active.GetBoolean()
            })
            .ToList();

        var readable = tabs
            .Take(12)
            .Select((tab, index) => $"{index + 1}. {(tab.Active ? "[ACTIVE] " : string.Empty)}{tab.Title ?? tab.Url ?? "Untitled tab"}")
            .ToArray();
        var summary = tabs.Count == 0
            ? "No browser tabs were found."
            : $"{tabs.Count} browser tab(s) detected:\n{string.Join("\n", readable)}";

        return new SkillResult(
            Id,
            SkillStatus.Completed,
            summary,
            new[] { new SkillStepResult("Read browser tabs", true, $"Received and parsed {tabs.Count} tab record(s).") });
    }
}
