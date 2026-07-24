using System.Text.Json;
using System.Windows;
using System.Windows.Media;
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
        AddLog("Gemini Live voice, text planner and local tool-calling engine loaded.");

        Closing += (_, _) =>
        {
            _voiceRequestedV12 = false;
            _ = _liveVoiceV12.StopAsync();
        };
    }

    private void UpdateVoiceStatusV12(string? state = null)
    {
        if (VoiceStatusText is null)
        {
            return;
        }
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
                SetMissionState("Jarvis is listening", "Speak naturally in English, Urdu, Roman Urdu or Hindi.", "LISTENING", Brushes.LightGreen);
            }
            if (state == "DISCONNECTED" && _voiceRequestedV12)
            {
                var settings = _settingsStoreV12.Load();
                if (settings.AutoReconnect && !_lifetime.IsCancellationRequested)
                {
                    UpdateVoiceStatusV12("RECONNECTING");
                    await Task.Delay(TimeSpan.FromSeconds(3));
                    if (_voiceRequestedV12 && !_lifetime.IsCancellationRequested)
                    {
                        await StartVoiceInternalV12();
                    }
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
        SetMissionState("Voice stopped", "Jarvis remains available through typed commands.", "READY", Brushes.DeepSkyBlue);
    }

    private void OpenSettingsV12()
    {
        var dialog = new SettingsWindow(_settingsStoreV12.Load()) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _settingsStoreV12.Save(dialog.Settings);
            UpdateVoiceStatusV12("READY");
            AddLog("Gemini and voice settings saved securely with Windows DPAPI.");
            SetMissionState("Settings saved", "Gemini intelligence is configured. Start Live Voice or enter a natural-language task.", "READY", Brushes.LightGreen);
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

            case "whatsapp_current_chat":
                var action = ReadStringV12(args, "action")?.ToLowerInvariant() ?? "inspect";
                var message = ReadStringV12(args, "message") ?? string.Empty;
                goal = action switch
                {
                    "draft" => "In the current WhatsApp chat, draft the requested message",
                    "send" => "In the current WhatsApp chat, send the requested message",
                    _ => "Inspect the current WhatsApp chat"
                };
                if (!string.IsNullOrWhiteSpace(message)) arguments["content"] = message;
                break;

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

    private static string? ReadStringV12(JsonElement args, string name) =>
        args.ValueKind == JsonValueKind.Object
        && args.TryGetProperty(name, out var value)
            ? value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()
            : null;

    private async void StartVoice_Click(object sender, RoutedEventArgs e) => await StartVoiceInternalV12();
    private async void StopVoice_Click(object sender, RoutedEventArgs e) => await StopVoiceInternalV12();
    private void OpenSettings_Click(object sender, RoutedEventArgs e) => OpenSettingsV12();
}