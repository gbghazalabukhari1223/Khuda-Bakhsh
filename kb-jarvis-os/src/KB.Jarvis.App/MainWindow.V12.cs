using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using KB.Jarvis.App.Native;
using KB.Jarvis.App.Services;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow
{
    private readonly JarvisSettingsStore _settingsStoreV12 = new();
    private readonly GeminiTextAgentService _textAgentV12 = new();
    private readonly GeminiLiveService _liveVoiceV12 = new();
    private readonly SpeechOutputService _speechV12 = new();
    private bool _voiceRequestedV12;
    private int _voiceConfirmationGuardV12;

    public void InitializeV12()
    {
        _liveVoiceV12.ToolExecutor = ExecuteGeminiToolAsyncV12;
        _liveVoiceV12.StateChanged += VoiceStateChangedV12;
        _liveVoiceV12.InputTranscript += InputTranscriptV12;
        _liveVoiceV12.OutputTranscript += OutputTranscriptV12;
        _liveVoiceV12.Diagnostic += message => Dispatcher.BeginInvoke(() => AddLog($"VOICE · {message}"));

        _skills.Register(new GeminiAgentSkill(
            _settingsStoreV12,
            _textAgentV12,
            _speechV12,
            ExecuteGeminiToolAsyncV12));
        SkillCountText.Text = $"{_skills.Skills.Count} TRAINED";
        UpdateVoiceStatusV12();
        AddLog("Gemini Live voice, multimodal text planner and local tool-calling engine loaded.");

        Closing += (_, _) =>
        {
            _voiceRequestedV12 = false;
            _ = _liveVoiceV12.StopAsync();
        };
    }

    private void UpdateVoiceStatusV12(string? state = null)
    {
        if (VoiceStatusText is null) return;
        var settings = _settingsStoreV12.Load();
        var resolved = state ?? (_liveVoiceV12.IsRunning ? "LISTENING" : string.IsNullOrWhiteSpace(settings.ApiKey) ? "NOT CONFIGURED" : "READY");
        VoiceStatusText.Text = resolved;
        VoiceStatusText.Foreground = resolved is "LISTENING" or "CONNECTED"
            ? Brushes.LightGreen
            : resolved is "CONNECTING" or "RECONNECTING"
                ? Brushes.Gold
                : resolved is "NOT CONFIGURED" or "ERROR"
                    ? Brushes.OrangeRed
                    : Brushes.DeepSkyBlue;
    }

    private void VoiceStateChangedV12(string state)
    {
        Dispatcher.BeginInvoke(async () =>
        {
            UpdateVoiceStatusV12(state);
            AddLog($"VOICE STATE · {state}");
            if (state == "LISTENING")
            {
                SetMissionState("Jarvis is listening", "Speak naturally. Enable screen or camera vision when visual context is required.", "LISTENING", Brushes.LightGreen);
            }
            if (state == "DISCONNECTED" && _voiceRequestedV12)
            {
                var settings = _settingsStoreV12.Load();
                if (settings.AutoReconnect && !_lifetime.IsCancellationRequested)
                {
                    UpdateVoiceStatusV12("RECONNECTING");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    if (_voiceRequestedV12 && !_lifetime.IsCancellationRequested) await StartVoiceInternalV12();
                }
            }
        });
    }

    private void InputTranscriptV12(string text)
    {
        Dispatcher.BeginInvoke(async () =>
        {
            AddLog($"Boss (voice): {text}");
            if (_pendingConfirmation is not null
                && IsConfirmationIntent(text)
                && Interlocked.CompareExchange(ref _voiceConfirmationGuardV12, 1, 0) == 0)
            {
                try
                {
                    var pending = _pendingConfirmation;
                    if (pending is not null)
                    {
                        var arguments = new Dictionary<string, string>(pending.Arguments, StringComparer.OrdinalIgnoreCase)
                        {
                            ["confirmed"] = "true"
                        };
                        _pendingConfirmation = null;
                        await ExecuteSkillRequestAsync(new SkillRequest(pending.Goal, arguments, _lifetime.Token));
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _voiceConfirmationGuardV12, 0);
                }
            }
        });
    }

    private void OutputTranscriptV12(string text)
    {
        Dispatcher.BeginInvoke(() =>
        {
            AddLog($"Jarvis (voice): {text}");
            MissionDetailText.Text = text;
        });
    }

    private async Task StartVoiceInternalV12()
    {
        try
        {
            var settings = _settingsStoreV12.Load();
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                UpdateVoiceStatusV12("NOT CONFIGURED");
                OpenSettingsV12();
                return;
            }
            _voiceRequestedV12 = true;
            UpdateVoiceStatusV12("CONNECTING");
            await _liveVoiceV12.StartAsync(settings, _lifetime.Token);
        }
        catch (Exception exception)
        {
            UpdateVoiceStatusV12("ERROR");
            AddLog($"VOICE START FAILED · {exception.Message}");
            SetMissionState("Voice connection failed", exception.Message, "ERROR", Brushes.OrangeRed);
        }
    }

    private async Task StopVoiceInternalV12()
    {
        _voiceRequestedV12 = false;
        await _liveVoiceV12.StopAsync();
        UpdateVoiceStatusV12("READY");
        SetMissionState("Voice stopped", "Jarvis remains available through typed and visual commands.", "READY", Brushes.DeepSkyBlue);
    }

    private void OpenSettingsV12()
    {
        var dialog = new SettingsWindow(_settingsStoreV12.Load()) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _settingsStoreV12.Save(dialog.Settings);
            UpdateVoiceStatusV12("READY");
            AddLog("Gemini and voice settings saved securely with Windows DPAPI.");
            SetMissionState("Settings saved", "Gemini intelligence is configured. Start voice, enable vision, or enter a natural-language task.", "READY", Brushes.LightGreen);
        }
    }

    private async Task<string> ExecuteGeminiToolAsyncV12(
        string functionName,
        JsonElement args,
        CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        string goal;
        switch (functionName)
        {
            case "execute_local_goal":
                goal = ReadStringV12(args, "goal") ?? throw new InvalidOperationException("The local goal was empty.");
                break;

            case "open_application":
                var application = ReadStringV12(args, "application") ?? throw new InvalidOperationException("Application name was missing.");
                if (application.Contains("whatsapp", StringComparison.OrdinalIgnoreCase))
                {
                    return "Blocked: WhatsApp messaging must use whatsapp_message and the existing signed-in tab. Jarvis did not open a new WhatsApp page.";
                }
                goal = $"Open {application}";
                break;

            case "write_notepad_note":
                var content = ReadStringV12(args, "content") ?? throw new InvalidOperationException("Notepad content was missing.");
                goal = "Open Notepad and write the requested note";
                arguments["content"] = content;
                var path = ReadStringV12(args, "path");
                if (!string.IsNullOrWhiteSpace(path)) arguments["path"] = path;
                break;

            case "list_browser_tabs":
                goal = "List active browser tabs";
                break;

            case "whatsapp_message":
            case "whatsapp_current_chat":
                var action = ReadStringV12(args, "action")?.ToLowerInvariant() ?? "inspect";
                var message = ReadStringV12(args, "message") ?? string.Empty;
                var contact = ReadStringV12(args, "contact") ?? string.Empty;
                goal = action switch
                {
                    "draft" => "Use the existing WhatsApp tab and draft the requested message",
                    "send" => "Use the existing WhatsApp tab and send the requested message",
                    _ => "Inspect the existing WhatsApp chat"
                };
                if (!string.IsNullOrWhiteSpace(message)) arguments["content"] = message;
                if (!string.IsNullOrWhiteSpace(contact)) arguments["contact"] = contact;
                break;

            case "search_files":
                goal = "Search files and folders on this PC";
                arguments["query"] = ReadStringV12(args, "query") ?? throw new InvalidOperationException("File search query was missing.");
                arguments["kind"] = ReadStringV12(args, "kind") ?? "any";
                arguments["open"] = ReadBoolV12(args, "open").ToString();
                break;

            case "windows_search":
                goal = "Use Windows search in the taskbar";
                arguments["query"] = ReadStringV12(args, "query") ?? throw new InvalidOperationException("Windows search query was missing.");
                arguments["open"] = ReadBoolV12(args, "open").ToString();
                break;

            case "desktop_input":
                return await ExecuteDesktopInputV13Async(args, cancellationToken).ConfigureAwait(false);

            default:
                throw new InvalidOperationException($"Unknown Jarvis tool: {functionName}");
        }

        var request = new SkillRequest(goal, arguments, cancellationToken);
        var result = await _skills.ExecuteAsync(request, "core.gemini-agent").ConfigureAwait(false);
        await Dispatcher.InvokeAsync(() =>
        {
            foreach (var step in result.Steps)
            {
                AddLog($"AI TOOL {(step.Success ? "PASS" : "FAIL")} · {step.Step} · {step.Evidence}");
            }
            if (result.RequiresConfirmation)
            {
                _pendingConfirmation = request;
                SetMissionState("Confirmation required", result.ConfirmationPrompt ?? result.Summary, "AWAITING APPROVAL", Brushes.Gold);
            }
            else
            {
                var brush = result.Status == SkillStatus.Completed ? Brushes.LightGreen : Brushes.Orange;
                SetMissionState("AI tool result", result.Summary, result.Status.ToString().ToUpperInvariant(), brush);
            }
        });

        return result.RequiresConfirmation
            ? $"Confirmation required from Boss before execution: {result.ConfirmationPrompt ?? result.Summary}"
            : $"{result.Status}: {result.Summary}";
    }

    private async Task<string> ExecuteDesktopInputV13Async(JsonElement args, CancellationToken cancellationToken)
    {
        var action = ReadStringV12(args, "action")?.ToLowerInvariant()
                     ?? throw new InvalidOperationException("Desktop input action was missing.");
        switch (action)
        {
            case "click":
            case "double_click":
            case "right_click":
                NativeInput.ClickNormalized(
                    ReadIntV12(args, "x"),
                    ReadIntV12(args, "y"),
                    action == "right_click" ? "right" : "left",
                    action == "double_click" ? 2 : 1);
                break;
            case "type":
                NativeInput.TypeUnicode(ReadStringV12(args, "text") ?? string.Empty);
                break;
            case "keypress":
                PressNamedKeyV13(ReadStringV12(args, "key") ?? string.Empty);
                break;
            case "scroll":
                NativeInput.Scroll(ReadIntV12(args, "amount", -3));
                break;
            default:
                throw new InvalidOperationException($"Unsupported desktop input action: {action}");
        }

        await Task.Delay(350, cancellationToken).ConfigureAwait(false);
        await Dispatcher.InvokeAsync(() => AddLog($"VISUAL INPUT · {action} executed."));
        return $"Native desktop action executed: {action}. Observe the next visual frame before deciding the next action.";
    }

    private static void PressNamedKeyV13(string key)
    {
        var normalized = key.Trim().ToUpperInvariant();
        switch (normalized)
        {
            case "ENTER": NativeInput.PressKey(0x0D); break;
            case "TAB": NativeInput.PressKey(0x09); break;
            case "ESCAPE": case "ESC": NativeInput.PressKey(0x1B); break;
            case "BACKSPACE": NativeInput.PressKey(0x08); break;
            case "DELETE": NativeInput.PressKey(0x2E); break;
            case "UP": NativeInput.PressKey(0x26); break;
            case "DOWN": NativeInput.PressKey(0x28); break;
            case "LEFT": NativeInput.PressKey(0x25); break;
            case "RIGHT": NativeInput.PressKey(0x27); break;
            case "WIN": NativeInput.PressKey(0x5B); break;
            case "CTRL_A": NativeInput.PressShortcut(0x11, 0x41); break;
            case "CTRL_C": NativeInput.PressShortcut(0x11, 0x43); break;
            case "CTRL_V": NativeInput.PressShortcut(0x11, 0x56); break;
            case "ALT_TAB": NativeInput.PressShortcut(0x12, 0x09); break;
            default: throw new InvalidOperationException($"Unsupported key name: {key}");
        }
    }

    private static string? ReadStringV12(JsonElement args, string name) =>
        args.ValueKind == JsonValueKind.Object && args.TryGetProperty(name, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;

    private static bool ReadBoolV12(JsonElement args, string name) =>
        args.ValueKind == JsonValueKind.Object
        && args.TryGetProperty(name, out var value)
        && (value.ValueKind == JsonValueKind.True
            || value.ValueKind == JsonValueKind.String && bool.TryParse(value.GetString(), out var parsed) && parsed);

    private static int ReadIntV12(JsonElement args, string name, int defaultValue = 0) =>
        args.ValueKind == JsonValueKind.Object
        && args.TryGetProperty(name, out var value)
        && (value.TryGetInt32(out var number) || int.TryParse(value.ToString(), out number))
            ? number
            : defaultValue;

    private async void StartVoice_Click(object sender, RoutedEventArgs e) => await StartVoiceInternalV12();
    private async void StopVoice_Click(object sender, RoutedEventArgs e) => await StopVoiceInternalV12();
    private void OpenSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsV12();
}