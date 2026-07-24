using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using KB.Jarvis.App.Core;
using KB.Jarvis.App.Native;
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
    private string _primaryModuleAction = string.Empty;
    private string _secondaryModuleAction = string.Empty;

    private static string JarvisRoot => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KB Jarvis OS");

    public MainWindow()
    {
        InitializeComponent();

        // Specific execution skills must be registered before the conversational fallback.
        _skills.Register(new NotepadWriteSkill());
        _skills.Register(new WhatsAppCurrentChatSkill(_browserBridge));
        _skills.Register(new BrowserTabsSkill(_browserBridge));
        _skills.Register(new AppLaunchSkill());
        _skills.Register(new ConversationSkill());

        SkillCountText.Text = $"{_skills.Skills.Count} TRAINED";
        AddLog($"{Identity.ProductName} {Identity.Version} initialized.");
        AddLog($"Developer identity locked: {Identity.DeveloperFull}.");
        AddLog("Interactive module controller and deterministic skill engine ready.");

        _connectionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _connectionTimer.Tick += (_, _) => RefreshBrowserStatus();

        PreviewKeyDown += (_, eventArgs) =>
        {
            if ((eventArgs.Key == Key.Space || eventArgs.SystemKey == Key.Space)
                && Keyboard.Modifiers.HasFlag(ModifierKeys.Alt))
            {
                eventArgs.Handled = true;
                ShowDashboard();
                CommandBox.Focus();
            }
        };

        Loaded += async (_, _) =>
        {
            if (TryFindResource("CoreAnimation") is Storyboard storyboard)
            {
                storyboard.Begin(this, true);
            }

            try
            {
                Directory.CreateDirectory(JarvisRoot);
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
                SetMissionState("Pending action cancelled", "No message was sent.", "READY", Brushes.LightGreen);
                SetModuleResult("Pending action cancelled. No message was sent.", success: true);
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

            SetMissionState(
                "Confirmation still required",
                "Type ‘Haan, bhej do’ to continue or ‘cancel’ to stop the pending action.",
                "AWAITING APPROVAL",
                Brushes.Gold);
            return;
        }

        SetMissionState(
            command,
            "Understanding the goal and selecting a verified workflow…",
            "PLANNING",
            Brushes.Gold);

        if (IsCreatorQuestion(command))
        {
            var answer = Identity.CreatorResponse();
            SetMissionState("Identity verified", answer, "COMPLETED", Brushes.LightGreen);
            SetModuleResult(answer, success: true);
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
        SetMissionState(
            request.Goal,
            "Executing locally and verifying every completed step…",
            "EXECUTING",
            Brushes.DeepSkyBlue);

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
            SetMissionState(
                "One confirmation required",
                result.ConfirmationPrompt ?? result.Summary,
                "AWAITING APPROVAL",
                Brushes.Gold);
            SetModuleResult(result.ConfirmationPrompt ?? result.Summary, success: true);
            AddLog($"CONFIRMATION · {result.ConfirmationPrompt ?? result.Summary}");
            return;
        }

        var title = result.Status switch
        {
            SkillStatus.Completed => "Mission completed and verified",
            SkillStatus.Prepared => "Jarvis response ready",
            SkillStatus.Blocked => "Mission blocked",
            _ => "Mission failed"
        };
        var brush = result.Status switch
        {
            SkillStatus.Completed => Brushes.LightGreen,
            SkillStatus.Prepared => Brushes.Gold,
            SkillStatus.Blocked => Brushes.Orange,
            _ => Brushes.OrangeRed
        };
        SetMissionState(title, result.Summary, result.Status.ToString().ToUpperInvariant(), brush);
        SetModuleResult(result.Summary, result.Status is SkillStatus.Completed or SkillStatus.Prepared);
        AddLog($"RESULT · {result.Status} · {result.Summary.Replace(Environment.NewLine, " | ")}");
    }

    private async Task ShowModuleAsync(string module)
    {
        if (string.Equals(module, "Dashboard", StringComparison.OrdinalIgnoreCase))
        {
            ShowDashboard();
            return;
        }

        DashboardSurface.Visibility = Visibility.Collapsed;
        ModuleSurface.Visibility = Visibility.Visible;
        ModuleTitleText.Text = $"{module.ToUpperInvariant()} MODULE";
        ModuleResultText.Text = "Select an operation to run it.";

        switch (module.ToLowerInvariant())
        {
            case "voice":
                ConfigureModule(
                    "Voice and Dictation Controls",
                    "WINDOWS VOICE READY",
                    "This module connects KB Jarvis to Windows speech settings and the native Windows voice-typing panel.",
                    "Open speech settings to configure microphones and language. Start dictation to speak directly into the Boss Command field. Full always-listening conversational voice remains a separate engine and is not falsely shown as active.",
                    "Open Speech Settings",
                    "Start Windows Dictation",
                    "voice.settings",
                    "voice.dictation");
                break;

            case "whatsapp":
                ConfigureModule(
                    "WhatsApp Executive Agent",
                    _browserBridge.IsConnected ? "COMPANION CONNECTED" : "COMPANION WAITING",
                    "Inspect the existing signed-in WhatsApp Web tab and operate the currently open chat through the Browser Companion.",
                    "The current-chat workflow reads the chat header, verifies the composer, drafts exact text, asks once before sending, and verifies the outgoing bubble. Contact search and attachments are not presented as complete in this patch.",
                    "Inspect Current Chat",
                    "Prepare Draft Command",
                    "whatsapp.inspect",
                    "whatsapp.compose");
                break;

            case "desktop":
                ConfigureModule(
                    "Native Desktop Controls",
                    "READY",
                    "Launch common Windows applications and run verified local skills without depending on browser DOM selectors.",
                    "The current verified desktop pack can open Notepad, Calculator, Paint, File Explorer, Task Manager, Settings, Chrome and Edge. The Notepad note workflow writes to disk, verifies exact content, and opens the result.",
                    "Run Notepad Test",
                    "Open File Explorer",
                    "desktop.notepad",
                    "desktop.explorer");
                break;

            case "browser":
                ConfigureModule(
                    "Active Browser Controls",
                    _browserBridge.IsConnected ? "CONNECTED" : "WAITING FOR EXTENSION",
                    "Use the installed Chrome Browser Companion to inspect existing tabs rather than opening a duplicate logged-out browser profile.",
                    "The tab operation returns active state, titles and URLs from the existing browser session. WhatsApp uses a page agent plus Chrome DevTools input for verified current-chat actions.",
                    "List Browser Tabs",
                    "Open Browser Home",
                    "browser.tabs",
                    "browser.home");
                break;

            case "vision":
                ConfigureModule(
                    "Screen and Vision Utilities",
                    "LOCAL UTILITIES READY",
                    "Use native Windows capture tools and inspect the virtual-screen geometry used by low-level input coordinates.",
                    "This button is deliberately labelled as a utility rather than claiming an unimplemented visual AI loop. It can open Snipping Tool and report the current virtual desktop dimensions for coordinate diagnostics.",
                    "Open Snipping Tool",
                    "Show Display Geometry",
                    "vision.snipping",
                    "vision.geometry");
                break;

            case "files":
                ConfigureModule(
                    "Files and Workspace",
                    "LOCAL STORAGE READY",
                    "Open KB Jarvis workspace and diagnostic folders with normal Windows File Explorer access.",
                    $"Jarvis local root: {JarvisRoot}. Generated notes, training templates, logs and future skill data are stored beneath this location or on the Desktop when explicitly requested.",
                    "Open Workspace",
                    "Open Logs Folder",
                    "files.workspace",
                    "files.logs");
                break;

            case "skills":
                ConfigureModule(
                    "Verified Skill Registry",
                    $"{_skills.Skills.Count} SKILLS LOADED",
                    "Review the workflows that this build can actually execute and verify.",
                    string.Join(Environment.NewLine, _skills.Skills.Select(skill => $"• {skill.DisplayName} ({skill.Id})")),
                    "Refresh Skill List",
                    "Show Command Examples",
                    "skills.list",
                    "skills.help");
                break;

            case "training":
                ConfigureModule(
                    "Teach and Training Workspace",
                    "TEMPLATE SYSTEM READY",
                    "Create a structured skill-training template and open the training folder for editing.",
                    "A training template records the goal, parameters, observable success condition, approval requirement, preferred execution method and recovery methods. This is a real persistent file, not a fake self-learning claim.",
                    "Create Training Template",
                    "Open Training Folder",
                    "training.create",
                    "training.folder");
                break;

            case "memory":
                ConfigureModule(
                    "Local Memory and Logs",
                    "PERSISTENT LOG READY",
                    "Open the current persistent Jarvis log or clear only the visible mission timeline.",
                    $"Current log file: {JarvisLog.CurrentLogPath}. Clearing the visible timeline does not delete the persistent diagnostic log.",
                    "Open Current Log",
                    "Clear Visible Timeline",
                    "memory.log",
                    "memory.clear");
                break;

            default:
                ConfigureModule(
                    module,
                    "UNKNOWN MODULE",
                    "This module name is not registered.",
                    "Return to the Dashboard and select a known module.",
                    "Back to Dashboard",
                    "Show Help",
                    "dashboard",
                    "skills.help");
                break;
        }

        AddLog($"Module opened: {module}.");
        await Task.CompletedTask;
    }

    private void ConfigureModule(
        string subtitle,
        string status,
        string description,
        string details,
        string primaryLabel,
        string secondaryLabel,
        string primaryAction,
        string secondaryAction)
    {
        ModuleSubtitleText.Text = subtitle;
        ModuleStatusText.Text = status;
        ModuleStatusText.Foreground = status.Contains("WAIT", StringComparison.OrdinalIgnoreCase)
            || status.Contains("UNKNOWN", StringComparison.OrdinalIgnoreCase)
            ? Brushes.Gold
            : Brushes.LightGreen;
        ModuleDescriptionText.Text = description;
        ModuleDetailsText.Text = details;
        PrimaryModuleActionButton.Content = primaryLabel;
        SecondaryModuleActionButton.Content = secondaryLabel;
        _primaryModuleAction = primaryAction;
        _secondaryModuleAction = secondaryAction;
    }

    private async Task RunModuleActionAsync(string action)
    {
        try
        {
            switch (action)
            {
                case "voice.settings":
                    OpenShellTarget("ms-settings:speech");
                    SetModuleResult("Windows speech settings opened.", success: true);
                    break;

                case "voice.dictation":
                    CommandBox.Focus();
                    NativeInput.PressShortcut(0x5B, 0x48); // Win + H
                    SetModuleResult("Windows voice typing requested for the Boss Command field.", success: true);
                    break;

                case "whatsapp.inspect":
                    await ExecutePresetCommandAsync("Inspect the current WhatsApp chat");
                    break;

                case "whatsapp.compose":
                    CommandBox.Text = "In the current WhatsApp chat, draft: \"Hello\"";
                    CommandBox.Focus();
                    CommandBox.SelectAll();
                    SetModuleResult("Draft command prepared. Replace “Hello” with the required message and press Enter.", success: true);
                    break;

                case "desktop.notepad":
                    await ExecutePresetCommandAsync("Open Notepad and write: \"KB Jarvis OS 11.1 interactive desktop test is working.\"");
                    break;

                case "desktop.explorer":
                    OpenShellTarget("explorer.exe");
                    SetModuleResult("File Explorer opened.", success: true);
                    break;

                case "browser.tabs":
                    await ExecutePresetCommandAsync("List active browser tabs");
                    break;

                case "browser.home":
                    OpenShellTarget("https://www.google.com");
                    SetModuleResult("The default browser was opened to Google.", success: true);
                    break;

                case "vision.snipping":
                    OpenShellTarget("snippingtool.exe");
                    SetModuleResult("Windows Snipping Tool opened.", success: true);
                    break;

                case "vision.geometry":
                    var geometry = $"Virtual screen: {SystemParameters.VirtualScreenWidth:0} × {SystemParameters.VirtualScreenHeight:0}; origin: {SystemParameters.VirtualScreenLeft:0}, {SystemParameters.VirtualScreenTop:0}.";
                    SetModuleResult(geometry, success: true);
                    AddLog($"VISION DIAGNOSTIC · {geometry}");
                    break;

                case "files.workspace":
                    OpenFolder(EnsureFolder("Workspace"));
                    break;

                case "files.logs":
                    OpenFolder(Path.GetDirectoryName(JarvisLog.CurrentLogPath) ?? JarvisRoot);
                    break;

                case "skills.list":
                    ModuleDetailsText.Text = string.Join(Environment.NewLine, _skills.Skills.Select(skill => $"• {skill.DisplayName}\n  {skill.Id}"));
                    SetModuleResult($"Refreshed {_skills.Skills.Count} registered skills.", success: true);
                    break;

                case "skills.help":
                    await ExecutePresetCommandAsync("help");
                    break;

                case "training.create":
                    await CreateTrainingTemplateAsync();
                    break;

                case "training.folder":
                    OpenFolder(EnsureFolder("Training"));
                    break;

                case "memory.log":
                    Directory.CreateDirectory(Path.GetDirectoryName(JarvisLog.CurrentLogPath) ?? JarvisRoot);
                    if (!File.Exists(JarvisLog.CurrentLogPath))
                    {
                        await File.WriteAllTextAsync(JarvisLog.CurrentLogPath, "KB Jarvis OS log initialized." + Environment.NewLine, _lifetime.Token);
                    }
                    OpenShellTarget(JarvisLog.CurrentLogPath);
                    SetModuleResult("The current persistent Jarvis log was opened.", success: true);
                    break;

                case "memory.clear":
                    MissionLog.Items.Clear();
                    SetModuleResult("The visible mission timeline was cleared. Persistent log files were not deleted.", success: true);
                    break;

                case "dashboard":
                    ShowDashboard();
                    break;

                default:
                    SetModuleResult("This module action is not implemented.", success: false);
                    break;
            }
        }
        catch (Exception exception)
        {
            AddLog($"MODULE ACTION FAILED · {action} · {exception.Message}");
            SetModuleResult(exception.Message, success: false);
        }
    }

    private async Task ExecutePresetCommandAsync(string command)
    {
        CommandBox.Text = command;
        await ExecuteCommandAsync();
    }

    private async Task CreateTrainingTemplateAsync()
    {
        var folder = EnsureFolder("Training");
        var path = Path.Combine(folder, $"skill-template-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        var template = new
        {
            name = "New KB Jarvis Skill",
            version = 1,
            goalExamples = new[] { "Describe the Boss's command here" },
            parameters = new[] { "text", "target" },
            successCondition = "Describe the observable evidence that proves completion",
            requiresConfirmation = false,
            preferredMethods = new[] { "semantic", "native-input", "visual-recovery" },
            recoveryMethods = new[] { "alternate-selector", "keyboard-navigation", "ask-for-missing-authentication" },
            developer = Identity.DeveloperFull
        };
        await File.WriteAllTextAsync(
            path,
            JsonSerializer.Serialize(template, new JsonSerializerOptions { WriteIndented = true }),
            _lifetime.Token);
        OpenShellTarget(path);
        SetModuleResult($"Training template created and opened: {path}", success: true);
        AddLog($"TRAINING TEMPLATE · {path}");
    }

    private static string EnsureFolder(string name)
    {
        var path = Path.Combine(JarvisRoot, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private void OpenFolder(string path)
    {
        Directory.CreateDirectory(path);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });
        SetModuleResult($"Opened folder: {path}", success: true);
    }

    private static void OpenShellTarget(string target)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = target,
            UseShellExecute = true
        });
    }

    private void ShowDashboard()
    {
        ModuleSurface.Visibility = Visibility.Collapsed;
        DashboardSurface.Visibility = Visibility.Visible;
        CommandBox.Focus();
        AddLog("Dashboard opened.");
    }

    private void SetMissionState(string title, string detail, string state, Brush brush)
    {
        MissionTitleText.Text = title;
        MissionDetailText.Text = detail;
        CoreStateText.Text = state;
        CoreStateText.Foreground = brush;
    }

    private void SetModuleResult(string message, bool success)
    {
        ModuleResultText.Text = message;
        ModuleResultText.Foreground = success ? Brushes.LightGreen : Brushes.OrangeRed;
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
            ? Brushes.LightGreen
            : Brushes.Gold;
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

    private async void ModuleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string module })
        {
            await ShowModuleAsync(module);
        }
    }

    private void BackToDashboard_Click(object sender, RoutedEventArgs e) => ShowDashboard();
    private async void PrimaryModuleAction_Click(object sender, RoutedEventArgs e) => await RunModuleActionAsync(_primaryModuleAction);
    private async void SecondaryModuleAction_Click(object sender, RoutedEventArgs e) => await RunModuleActionAsync(_secondaryModuleAction);

    private async void NotepadDemo_Click(object sender, RoutedEventArgs e) =>
        await ExecutePresetCommandAsync("Open Notepad and write: \"KB Jarvis OS 11.1 Notepad skill is working.\"");

    private async void WhatsAppInspect_Click(object sender, RoutedEventArgs e) =>
        await ExecutePresetCommandAsync("Inspect the current WhatsApp chat");

    private async void BrowserTabs_Click(object sender, RoutedEventArgs e) =>
        await ExecutePresetCommandAsync("List active browser tabs");

    private async void Identity_Click(object sender, RoutedEventArgs e) =>
        await ExecutePresetCommandAsync("Who created you?");

    private void BrowserTest_Click(object sender, RoutedEventArgs e)
    {
        RefreshBrowserStatus();
        var message = _browserBridge.IsConnected
            ? $"Browser Companion command channel is connected on port {_browserBridge.Port}."
            : $"Browser Companion is waiting on port {_browserBridge.Port}. Reload the Version 11.1 extension.";
        AddLog(message);
        SetModuleResult(message, _browserBridge.IsConnected);
        SetMissionState("Browser Companion test", message, _browserBridge.IsConnected ? "COMPLETED" : "WAITING", _browserBridge.IsConnected ? Brushes.LightGreen : Brushes.Gold);
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
