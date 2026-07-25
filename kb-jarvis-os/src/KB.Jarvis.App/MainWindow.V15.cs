using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow
{
    private int _activeBatchV15;

    public void InitializeV15()
    {
        InitializeV13();
        Title = "KB Jarvis OS 15 — Female Executive";
        ApplyV15Branding(this);
        AddLog("Jarvis 15 female executive persona, fluent voice transport and verified multi-task queue loaded.");
        SetMissionState(
            "Jarvis 15 is ready, Boss.",
            "Give one task or a complete sequence. I will execute supported work in order and verify each result.",
            "FEMALE EXECUTIVE ONLINE",
            Brushes.MediumSpringGreen);
    }

    private static void ApplyV15Branding(DependencyObject root)
    {
        if (root is TextBlock text)
        {
            text.Text = text.Text
                .Replace("KB JARVIS OS 14", "KB JARVIS OS 15", StringComparison.Ordinal)
                .Replace("V14 OPERATOR ONLINE", "V15 FEMALE EXECUTIVE ONLINE", StringComparison.Ordinal)
                .Replace("Version 14.0.0", "Version 15.0.0", StringComparison.Ordinal)
                .Replace("V14 WORK CORE", "V15 EXECUTIVE CORE", StringComparison.Ordinal)
                .Replace("HIGH-SPEED WORK CORE", "FLUENT FEMALE MULTI-TASK CORE", StringComparison.Ordinal)
                .Replace("VOICE QUEUE", "FLUENT VOICE", StringComparison.Ordinal)
                .Replace("OBSERVE · ACT · VERIFY", "UNDERSTAND · EXECUTE · VERIFY", StringComparison.Ordinal);
        }

        var children = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < children; index++)
        {
            ApplyV15Branding(VisualTreeHelper.GetChild(root, index));
        }
    }

    private async Task<string> ExecuteTaskBatchV15Async(JsonElement args, CancellationToken cancellationToken)
    {
        if (Interlocked.CompareExchange(ref _activeBatchV15, 1, 0) != 0)
        {
            return "Blocked: another multi-task mission is already running. Wait for its verified completion or cancel it first.";
        }

        try
        {
            if (args.ValueKind != JsonValueKind.Object
                || !args.TryGetProperty("tasks", out var tasksElement)
                || tasksElement.ValueKind != JsonValueKind.Array)
            {
                return "Blocked: execute_task_batch requires a tasks array.";
            }

            var tasks = tasksElement
                .EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()?.Trim())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Cast<string>()
                .Take(12)
                .ToArray();

            if (tasks.Length == 0)
            {
                return "Blocked: the multi-task mission contained no executable tasks.";
            }

            var stopOnFailure = !args.TryGetProperty("continue_on_failure", out var continueElement)
                                || continueElement.ValueKind != JsonValueKind.True;
            var report = new StringBuilder();
            var completed = 0;
            var failed = 0;

            await Dispatcher.InvokeAsync(() =>
            {
                AddLog($"MISSION QUEUE · {tasks.Length} task(s) accepted.");
                SetMissionState(
                    "Multi-task mission running",
                    $"0 of {tasks.Length} verified. Jarvis will preserve task order and application focus.",
                    "QUEUE ACTIVE",
                    Brushes.DeepSkyBlue);
            });

            for (var index = 0; index < tasks.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var task = tasks[index];
                await Dispatcher.InvokeAsync(() =>
                {
                    AddLog($"QUEUE {index + 1}/{tasks.Length} · START · {task}");
                    MissionDetailText.Text = $"Executing {index + 1} of {tasks.Length}: {task}";
                    CoreStateText.Text = $"QUEUE {index + 1}/{tasks.Length}";
                });

                var request = new SkillRequest(
                    task,
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                    cancellationToken);
                var result = await _skills.ExecuteAsync(request, "core.gemini-agent").ConfigureAwait(false);

                report.AppendLine($"{index + 1}. {task}");
                report.AppendLine($"   {result.Status}: {result.Summary}");

                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var step in result.Steps)
                    {
                        AddLog($"QUEUE {index + 1}/{tasks.Length} · {(step.Success ? "PASS" : "FAIL")} · {step.Step} · {step.Evidence}");
                    }
                });

                if (result.RequiresConfirmation)
                {
                    _pendingConfirmation = request;
                    await Dispatcher.InvokeAsync(() =>
                    {
                        SetMissionState(
                            "Mission queue paused for approval",
                            result.ConfirmationPrompt ?? result.Summary,
                            $"WAITING · TASK {index + 1}/{tasks.Length}",
                            Brushes.Gold);
                        AddLog($"QUEUE PAUSED · Confirmation required before task {index + 1} can continue.");
                    });
                    report.AppendLine("   Queue paused for Boss confirmation. Remaining tasks were not executed yet.");
                    return report.ToString().Trim();
                }

                if (result.Status == SkillStatus.Completed)
                {
                    completed++;
                }
                else
                {
                    failed++;
                    if (stopOnFailure)
                    {
                        report.AppendLine("   Queue stopped because this task did not complete successfully.");
                        break;
                    }
                }
            }

            await Dispatcher.InvokeAsync(() =>
            {
                var colour = failed == 0 ? Brushes.LightGreen : Brushes.Orange;
                SetMissionState(
                    "Multi-task mission finished",
                    $"Verified completed: {completed}. Not completed: {failed}.",
                    failed == 0 ? "QUEUE COMPLETE" : "QUEUE PARTIAL",
                    colour);
                AddLog($"MISSION QUEUE · FINISHED · completed={completed}, not-completed={failed}");
            });

            return report.ToString().Trim();
        }
        finally
        {
            Interlocked.Exchange(ref _activeBatchV15, 0);
        }
    }
}
