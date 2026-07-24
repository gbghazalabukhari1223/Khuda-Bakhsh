using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using KB.Jarvis.App.Core;
using KB.Jarvis.App.Services;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow : Window
{
    private readonly SkillRegistry _skills = new();
    private readonly BrowserBridgeService _browserBridge = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly DispatcherTimer _connectionTimer;
    private SkillRequest? _pendingConfirmation;

    public MainWindow()
    {
        InitializeComponent();
        _skills.Register(new NotepadWriteSkill());
        _skills.Register(new WhatsAppCurrentChatSkill(_browserBridge));
        SkillCountText.Text = $"{_skills.Skills.Count} TRAINED";
        AddLog($"{Identity.ProductName} {Identity.Version} initialized.");
        AddLog($"Developer identity locked: {Identity.DeveloperFull}.");
        AddLog("Native deterministic skill engine ready.");

        _connectionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _connectionTimer.Tick += (_, _) => RefreshBrowserStatus();

        Loaded += async (_, _) =>
        {
            if (TryFindResource("CoreAnimation") is Storyboard storyboard)
            {
                storyboard.Begin(this, true);
            }

            try
            {
                await _browserBridge.StartAsync(_lifetime.Token);
                AddLog($"Native Browser Companion bridge listening on 127.0.0.1:{_browserBridge.Port}.");
            }
            catch (Exception exception)
            {
                AddLog($"Browser bridge could not start: {exception.Message}");
            }

            RefreshBrowserStatus();
            _connectionTimer.Start();
            CommandBox.Focus();
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        _connectionTimer.Stop();
        _lifetime.Cancel();
        _ = _browserBridge.DisposeAsync();
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
        AddLog($"Boss: {command}");

        if (_pendingConfirmation is not null)
        {
            if (IsCancellationIntent(command))
            {
                AddLog("Pending consequential action cancelled by Boss.");
                _pendingConfirmation = null;
                MissionTitleText.Text = "Pending action cancelled";
                MissionDetailText.Text = "No message was sent.";
                CoreStateText.Text = "READY";
                return;
            }

            if (IsConfirmationIntent(command))
            {
                var confirmedArguments = new Dictionary<string, string>(
                    _pendingConfirmation.Arguments,
                    StringComparer.OrdinalIgnoreCase)
                {
                    ["confirmed"] = "true"
                };
                var confirmedRequest = new SkillRequest(
                    _pendingConfirmation.Goal,
                    confirmedArguments,
                    _lifetime.Token);
                _pendingConfirmation = null;
                await ExecuteSkillRequestAsync(confirmedRequest);
                return;
            }
        }

        MissionTitleText.Text = command;
        MissionDetailText.Text = "Understanding goal and selecting a trained execution workflow…";
        CoreStateText.Text = "PLANNING";
        CoreStateText.Foreground = System.Windows.Media.Brushes.Gold;

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

        if (IsMinimizeIntent(command))
        {
            AddLog("Entering minimized worker mode. Windows remains running normally.");
            WindowState = WindowState.Minimized;
            return;
        }

        if (IsCloseJarvisIntent(command))
        {
            AddLog("Closing KB Jarvis only. No Windows power action was requested or executed.");
            Application.Current.Shutdown();
            return;
        }

        var commandArguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var quoted = ExtractQuotedText(command);
        if (!string.IsNullOrWhiteSpace(quoted))
        {
            commandArguments["content"] = quoted;
        }

        await ExecuteSkillRequestAsync(new SkillRequest(command, commandArguments, _lifetime.Token));
    }

    private async Task ExecuteSkillRequestAsync(SkillRequest request)
    {
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

        if (result.RequiresConfirmation)
        {
            _pendingConfirmation = request;
            MissionTitleText.Text = "One confirmation required";
            MissionDetailText.Text = result.ConfirmationPrompt ?? result.Summary;
            CoreStateText.Text = "AWAITING APPROVAL";
            CoreStateText.Foreground = System.Windows.Media.Brushes.Gold;
            AddLog($"CONFIRMATION · {result.ConfirmationPrompt ?? result.Summary}");
            return;
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

    private static bool IsMinimizeIntent(string command)
    {
        var normalized = command.ToLowerInvariant();
        return normalized.Contains("minimize yourself")
               || normalized.Contains("minimize jarvis")
               || normalized.Contains("khud ko minimize")
               || normalized.Contains("background mode");
    }

    private static bool IsCloseJarvisIntent(string command)
    {
        var normalized = command.ToLowerInvariant();
        var namesJarvis = normalized.Contains("jarvis")
                          || normalized.Contains("your app")
                          || normalized.Contains("yourself")
                          || normalized.Contains("apni app");
        var asksClose = normalized.Contains("close")
                        || normalized.Contains("exit")
                        || normalized.Contains("band ho")
                        || normalized.Contains("band karo");
        return namesJarvis && asksClose;
    }

    private static bool IsConfirmationIntent(string command)
    {
        var normalized = command.Trim().ToLowerInvariant();
        return normalized is "yes" or "ok" or "confirm" or "send" or "send it" or "haan" or "han" or "bhej do" or "kr do" or "kar do"
               || normalized.Contains("yes send")
               || normalized.Contains("haan bhej");
    }

    private static bool IsCancellationIntent(string command)
    {
        var normalized = command.Trim().ToLowerInvariant();
        return normalized is "cancel" or "no" or "nahi" or "mat bhejo" or "don't send";
    }

    private static string? ExtractQuotedText(string command)
    {
        var match = Regex.Match(command, """["“”'](?<text>.+?)["“”']""", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups["text"].Value;
        }

        var markerIndex = command.IndexOf(':');
        return markerIndex >= 0 && markerIndex + 1 < command.Length
            ? command[(markerIndex + 1)..].Trim()
            : null;
    }

    private void RefreshBrowserStatus()
    {
        BrowserStatusText.Text = _browserBridge.IsConnected
            ? $"CONNECTED :{_browserBridge.Port}"
            : $"WAITING :{_browserBridge.Port}";
        BrowserStatusText.Foreground = _browserBridge.IsConnected
            ? System.Windows.Media.Brushes.LightGreen
            : System.Windows.Media.Brushes.Gold;
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

    private void BrowserTest_Click(object sender, RoutedEventArgs e)
    {
        RefreshBrowserStatus();
        AddLog(_browserBridge.IsConnected
            ? "Browser Companion command channel is connected and ready."
            : "Browser Companion is not connected yet. Reload the Version 11 extension; it will reconnect automatically.");
    }

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
