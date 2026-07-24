using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using KB.Jarvis.App.Core;
using KB.Jarvis.App.Services;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow : Window
{
    private readonly SkillRegistry _skills = new();
    private readonly BrowserCompanionProbe _browserProbe = new();
    private readonly CancellationTokenSource _lifetime = new();

    public MainWindow()
    {
        InitializeComponent();
        _skills.Register(new NotepadWriteSkill());
        SkillCountText.Text = $"{_skills.Skills.Count} TRAINED";
        AddLog($"{Identity.ProductName} {Identity.Version} initialized.");
        AddLog($"Developer identity locked: {Identity.DeveloperFull}.");
        AddLog("Native deterministic skill engine ready.");

        Loaded += async (_, _) =>
        {
            if (TryFindResource("CoreAnimation") is Storyboard storyboard)
            {
                storyboard.Begin(this, true);
            }

            await RefreshBrowserStatusAsync();
            CommandBox.Focus();
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        base.OnClosed(e);
    }

    private async Task ExecuteCommandAsync()
    {
        var command = CommandBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        CommandBox.Clear();
        MissionTitleText.Text = command;
        MissionDetailText.Text = "Understanding goal and selecting a trained execution workflow…";
        CoreStateText.Text = "PLANNING";
        CoreStateText.Foreground = System.Windows.Media.Brushes.Gold;
        AddLog($"Boss: {command}");

        if (IsCreatorQuestion(command))
        {
            var answer = Identity.CreatorResponse();
            MissionTitleText.Text = "Identity verified";
            MissionDetailText.Text = answer;
            CoreStateText.Text = "COMPLETED";
            CoreStateText.Foreground = System.Windows.Media.Brushes.LightGreen;
            AddLog($"Jarvis: {answer}");
            return;
        }

        var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var quoted = ExtractQuotedText(command);
        if (!string.IsNullOrWhiteSpace(quoted))
        {
            arguments["content"] = quoted;
        }

        var request = new SkillRequest(command, arguments, _lifetime.Token);
        CoreStateText.Text = "EXECUTING";
        CoreStateText.Foreground = System.Windows.Media.Brushes.DeepSkyBlue;
        MissionDetailText.Text = "Executing locally and verifying every completed step…";

        var result = await _skills.ExecuteAsync(request);
        foreach (var step in result.Steps)
        {
            AddLog($"{(step.Success ? "PASS" : "FAIL")} · {step.Step} · {step.Evidence}");
            if (!string.IsNullOrWhiteSpace(step.RecoveryMethod))
            {
                AddLog($"RECOVERY · {step.RecoveryMethod}");
            }
        }

        MissionTitleText.Text = result.Status switch
        {
            SkillStatus.Completed => "Mission completed and verified",
            SkillStatus.Prepared => "Mission prepared with a remaining blocker",
            SkillStatus.Blocked => "Mission blocked",
            _ => "Mission failed"
        };
        MissionDetailText.Text = result.Summary;
        CoreStateText.Text = result.Status.ToString().ToUpperInvariant();
        CoreStateText.Foreground = result.Status switch
        {
            SkillStatus.Completed => System.Windows.Media.Brushes.LightGreen,
            SkillStatus.Prepared => System.Windows.Media.Brushes.Gold,
            SkillStatus.Blocked => System.Windows.Media.Brushes.Orange,
            _ => System.Windows.Media.Brushes.OrangeRed
        };
        AddLog($"RESULT · {result.Status} · {result.Summary}");
    }

    private static bool IsCreatorQuestion(string command)
    {
        var normalized = command.ToLowerInvariant();
        return normalized.Contains("who created you")
               || normalized.Contains("who made you")
               || normalized.Contains("your developer")
               || normalized.Contains("kis ne banaya")
               || normalized.Contains("kiss ne banaya")
               || normalized.Contains("کس نے بنایا")
               || normalized.Contains("developer kon");
    }

    private static string? ExtractQuotedText(string command)
    {
        var match = Regex.Match(command, "[\"“”'](?<text>.+?)[\"“”']", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups["text"].Value;
        }

        var markerIndex = command.IndexOf(':');
        return markerIndex >= 0 && markerIndex + 1 < command.Length
            ? command[(markerIndex + 1)..].Trim()
            : null;
    }

    private async Task RefreshBrowserStatusAsync()
    {
        BrowserStatusText.Text = "CHECKING";
        BrowserStatusText.Foreground = System.Windows.Media.Brushes.Gold;
        var health = await _browserProbe.CheckAsync(_lifetime.Token);
        BrowserStatusText.Text = health.Connected ? $"ONLINE :{health.Port}" : "OFFLINE";
        BrowserStatusText.Foreground = health.Connected
            ? System.Windows.Media.Brushes.LightGreen
            : System.Windows.Media.Brushes.OrangeRed;
        AddLog(health.Connected
            ? $"Browser Companion bridge detected on port {health.Port}."
            : health.Detail);
    }

    private void AddLog(string text)
    {
        MissionLog.Items.Insert(0, $"{DateTime.Now:HH:mm:ss}  {text}");
        while (MissionLog.Items.Count > 120)
        {
            MissionLog.Items.RemoveAt(MissionLog.Items.Count - 1);
        }
        JarvisLog.Info(text);
    }

    private async void Execute_Click(object sender, RoutedEventArgs e) => await ExecuteCommandAsync();

    private async void CommandBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
        {
            e.Handled = true;
            await ExecuteCommandAsync();
        }
    }

    private async void NotepadDemo_Click(object sender, RoutedEventArgs e)
    {
        CommandBox.Text = "Open Notepad and write: \"KB Jarvis OS native Notepad skill is working.\"";
        await ExecuteCommandAsync();
    }

    private void Identity_Click(object sender, RoutedEventArgs e)
    {
        CommandBox.Text = "Who created you?";
        _ = ExecuteCommandAsync();
    }

    private async void BrowserTest_Click(object sender, RoutedEventArgs e) => await RefreshBrowserStatusAsync();

    private void ClearLog_Click(object sender, RoutedEventArgs e) => MissionLog.Items.Clear();

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        DragMove();
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void ToggleMaximize() => WindowState = WindowState == WindowState.Maximized
        ? WindowState.Normal
        : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        AddLog("Closing KB Jarvis only. Windows power state is unchanged.");
        Application.Current.Shutdown();
    }
}
