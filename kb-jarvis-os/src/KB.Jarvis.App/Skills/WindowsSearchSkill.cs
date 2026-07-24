using KB.Jarvis.App.Native;

namespace KB.Jarvis.App.Skills;

public sealed class WindowsSearchSkill : IJarvisSkill
{
    public string Id => "desktop.windows-search";
    public string DisplayName => "Desktop — Taskbar and Start Search";

    public bool CanHandle(SkillRequest request)
    {
        if (request.Arguments.ContainsKey("query")
            && request.Goal.Contains("Windows search", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("taskbar search")
               || goal.Contains("windows search")
               || goal.Contains("start menu search")
               || goal.Contains("search box in taskbar")
               || goal.Contains("task bar men search")
               || goal.Contains("taskbar mein search");
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var query = request.Arguments.TryGetValue("query", out var supplied)
            ? supplied.Trim()
            : ExtractQuery(request.Goal);
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SkillResult(Id, SkillStatus.Blocked, "A Windows search term is required.", Array.Empty<SkillStepResult>());
        }

        var openRequested = request.Arguments.TryGetValue("open", out var raw)
                            && bool.TryParse(raw, out var open)
                            && open;

        NativeInput.PressKey(0x5B); // Windows key
        await Task.Delay(550, request.CancellationToken).ConfigureAwait(false);
        NativeInput.TypeUnicode(query);
        await Task.Delay(650, request.CancellationToken).ConfigureAwait(false);
        if (openRequested)
        {
            NativeInput.PressKey(0x0D); // Enter
        }

        var summary = openRequested
            ? $"Windows Search received “{query}” and the top result was opened with Enter."
            : $"Windows Search received “{query}”. Results remain visible for review.";
        return new SkillResult(
            Id,
            SkillStatus.Completed,
            summary,
            new[]
            {
                new SkillStepResult("Open Windows Search", true, "The Windows key was dispatched."),
                new SkillStepResult("Enter search query", true, $"Unicode text was dispatched: {query}"),
                new SkillStepResult("Open top result", !openRequested || openRequested, openRequested ? "Enter was dispatched." : "Opening was not requested.")
            });
    }

    private static string ExtractQuery(string goal)
    {
        var colon = goal.IndexOf(':');
        if (colon >= 0 && colon + 1 < goal.Length)
        {
            return goal[(colon + 1)..].Trim(' ', '"', '\'', '“', '”');
        }

        var result = goal;
        var phrases = new[]
        {
            "taskbar search", "windows search", "start menu search", "search box in taskbar",
            "task bar men search", "taskbar mein search", "search", "karo", "kro", "please", "for"
        };
        foreach (var phrase in phrases)
        {
            result = result.Replace(phrase, " ", StringComparison.OrdinalIgnoreCase);
        }
        return string.Join(' ', result.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }
}