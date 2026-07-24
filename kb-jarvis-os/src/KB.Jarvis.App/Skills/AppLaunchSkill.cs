using System.Diagnostics;

namespace KB.Jarvis.App.Skills;

public sealed class AppLaunchSkill : IJarvisSkill
{
    private sealed record LaunchTarget(string Name, string FileName, string? Arguments = null);

    private static readonly IReadOnlyDictionary<string, LaunchTarget> Targets =
        new Dictionary<string, LaunchTarget>(StringComparer.OrdinalIgnoreCase)
        {
            ["notepad"] = new("Notepad", "notepad.exe"),
            ["calculator"] = new("Calculator", "calc.exe"),
            ["calc"] = new("Calculator", "calc.exe"),
            ["paint"] = new("Paint", "mspaint.exe"),
            ["file explorer"] = new("File Explorer", "explorer.exe"),
            ["explorer"] = new("File Explorer", "explorer.exe"),
            ["task manager"] = new("Task Manager", "taskmgr.exe"),
            ["settings"] = new("Windows Settings", "ms-settings:"),
            ["chrome"] = new("Google Chrome", "chrome.exe"),
            ["google chrome"] = new("Google Chrome", "chrome.exe"),
            ["edge"] = new("Microsoft Edge", "msedge.exe"),
            ["microsoft edge"] = new("Microsoft Edge", "msedge.exe"),
            ["visual studio code"] = new("Visual Studio Code", "code"),
            ["vs code"] = new("Visual Studio Code", "code"),
            ["vscode"] = new("Visual Studio Code", "code")
        };

    public string Id => "desktop.app.launch";
    public string DisplayName => "Desktop — Launch Common Windows Apps";

    public bool CanHandle(SkillRequest request)
    {
        var goal = request.Goal.ToLowerInvariant();
        if (goal.Contains("whatsapp") || goal.Contains("youtube")) return false;

        var hasLaunchVerb = goal.Contains("open")
                            || goal.Contains("launch")
                            || goal.Contains("start")
                            || goal.Contains("kholo")
                            || goal.Contains("khol do")
                            || goal.Contains("چلاؤ")
                            || goal.Contains("کھولو");
        return hasLaunchVerb && TryResolve(goal, out _);
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        if (!TryResolve(request.Goal.ToLowerInvariant(), out var target) || target is null)
        {
            return new SkillResult(
                Id,
                SkillStatus.Blocked,
                "The requested application is not in the verified launch list.",
                Array.Empty<SkillStepResult>());
        }

        var steps = new List<SkillStepResult>();
        Process? process;
        try
        {
            process = Process.Start(new ProcessStartInfo
            {
                FileName = target.FileName,
                Arguments = target.Arguments ?? string.Empty,
                UseShellExecute = true
            });
        }
        catch (Exception exception)
        {
            steps.Add(new SkillStepResult("Launch application", false, exception.Message));
            return new SkillResult(Id, SkillStatus.Failed, $"{target.Name} could not be launched.", steps);
        }

        await Task.Delay(350, request.CancellationToken).ConfigureAwait(false);
        var started = process is not null || target.FileName.StartsWith("ms-settings:", StringComparison.OrdinalIgnoreCase);
        steps.Add(new SkillStepResult(
            "Launch application",
            started,
            started ? $"Windows accepted the launch request for {target.Name}." : "Windows did not return a launch result."));

        return new SkillResult(
            Id,
            started ? SkillStatus.Completed : SkillStatus.Failed,
            started ? $"{target.Name} was opened." : $"{target.Name} could not be opened.",
            steps);
    }

    private static bool TryResolve(string goal, out LaunchTarget? target)
    {
        foreach (var pair in Targets.OrderByDescending(pair => pair.Key.Length))
        {
            if (goal.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                target = pair.Value;
                return true;
            }
        }

        target = null;
        return false;
    }
}