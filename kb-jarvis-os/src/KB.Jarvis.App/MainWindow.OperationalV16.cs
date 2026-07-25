using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using KB.Jarvis.App.Services;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow
{
    private readonly StableGeminiLiveService _operationalVoiceV16 = new();
    private bool _operationalVoiceRequestedV16;
    private int _operationalVoiceConfirmationGuardV16;
    private int _operationalV16Initialized;
    private long _lastOperationalVisualTicksV16;

    static MainWindow()
    {
        EventManager.RegisterClassHandler(
            typeof(Button),
            ButtonBase.ClickEvent,
            new RoutedEventHandler(OperationalButtonClickV16),
            handledEventsToo: true);
    }

    public void InitializeOperationalV16()
    {
        if (Interlocked.Exchange(ref _operationalV16Initialized, 1) == 1) return;

        _operationalVoiceV16.ToolExecutor = ExecuteOperationalToolV16Async;
        _operationalVoiceV16.ContextProvider = OperationalContextStore.CreateModelContext;
        _operationalVoiceV16.StateChanged += OperationalVoiceStateChangedV16;
        _operationalVoiceV16.InputTranscript += OperationalInputTranscriptV16;
        _operationalVoiceV16.OutputTranscript += OperationalOutputTranscriptV16;
        _operationalVoiceV16.Diagnostic += message =>
            Dispatcher.BeginInvoke(() => AddLog($"VOICE STABILITY · {message}"));
        _visualV13.FrameReady += OperationalVisualFrameReadyV16;

        Closing += (_, _) =>
        {
            _operationalVoiceRequestedV16 = false;
            _ = _operationalVoiceV16.StopAsync();
        };

        Title = "KB Jarvis OS 15 — Operational Female Executive";
        OperationalContextStore.RecordState(
            "READY",
            "Operational continuity, stable voice and preserved capability layer loaded",
            expectedPostcondition: "Every supported task is executed and independently verified");
        AddLog("Jarvis operational upgrade loaded without removing previous files, websites, WordPress, WhatsApp, YouTube, browser, vision, desktop or multi-task capabilities.");
        AddLog("Stable voice routing is now authoritative for START LISTENING and STOP VOICE.");
        SetMissionState(
            "Operational Jarvis is ready, Boss.",
            "Previous capabilities are preserved. Give one real task or a complete ordered mission; Jarvis will inspect, execute, verify and retain the operational checkpoint.",
            "OPERATIONAL READY",
            Brushes.LightGreen);
    }

    private static void OperationalButtonClickV16(object sender, RoutedEventArgs eventArgs)
    {
        if (sender is not Button button
            || Window.GetWindow(button) is not MainWindow window)
        {
            return;
        }

        var label = button.Content?.ToString()?.Trim();
        if (string.Equals(label, "START LISTENING", StringComparison.OrdinalIgnoreCase))
        {
            eventArgs.Handled = true;
            _ = window.StartOperationalVoiceV16Async();
        }
        else if (string.Equals(label, "STOP VOICE", StringComparison.OrdinalIgnoreCase))
        {
            eventArgs.Handled = true;
            _ = window.StopOperationalVoiceV16Async();
        }
    }

    private async Task StartOperationalVoiceV16Async()
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

            if (_liveVoiceV12.IsRunning)
            {
                await _liveVoiceV12.StopAsync().ConfigureAwait(false);
            }

            _operationalVoiceRequestedV16 = true;
            await Dispatcher.InvokeAsync(() => UpdateVoiceStatusV12("CONNECTING"));
            await _operationalVoiceV16.StartAsync(settings, _lifetime.Token).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            SpeechModeCoordinator.SetLiveVoiceActive(false);
            await Dispatcher.InvokeAsync(() =>
            {
                UpdateVoiceStatusV12("ERROR");
                AddLog($"STABLE VOICE START FAILED · {exception.Message}");
                SetMissionState("Voice connection failed", exception.Message, "ERROR", Brushes.OrangeRed);
            });
        }
    }

    private async Task StopOperationalVoiceV16Async()
    {
        _operationalVoiceRequestedV16 = false;
        await _operationalVoiceV16.StopAsync().ConfigureAwait(false);
        await Dispatcher.InvokeAsync(() =>
        {
            UpdateVoiceStatusV12("READY");
            SetMissionState(
                "Voice stopped",
                "Jarvis remains fully available through typed commands, browser operations, files, websites, WordPress, WhatsApp and visual workflows.",
                "READY",
                Brushes.DeepSkyBlue);
        });
    }

    private void OperationalVoiceStateChangedV16(string state)
    {
        Dispatcher.BeginInvoke(async () =>
        {
            UpdateVoiceStatusV12(state);
            AddLog($"STABLE VOICE STATE · {state}");
            OperationalContextStore.RecordState(state, $"Stable voice state changed to {state}");

            if (state == "LISTENING")
            {
                SetMissionState(
                    "Jarvis is listening, Boss.",
                    "Speak naturally. Stable buffering and single-engine output are active.",
                    "LISTENING",
                    Brushes.LightGreen);
            }

            if (state == "DISCONNECTED" && _operationalVoiceRequestedV16)
            {
                var settings = _settingsStoreV12.Load();
                if (settings.AutoReconnect && !_lifetime.IsCancellationRequested)
                {
                    UpdateVoiceStatusV12("RECONNECTING");
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    if (_operationalVoiceRequestedV16 && !_lifetime.IsCancellationRequested)
                    {
                        await StartOperationalVoiceV16Async();
                    }
                }
            }
        });
    }

    private void OperationalInputTranscriptV16(string text)
    {
        OperationalContextStore.RecordVoiceInput(text);
        Dispatcher.BeginInvoke(async () =>
        {
            AddLog($"Boss (stable voice): {text}");
            if (_pendingConfirmation is not null
                && IsConfirmationIntent(text)
                && Interlocked.CompareExchange(ref _operationalVoiceConfirmationGuardV16, 1, 0) == 0)
            {
                try
                {
                    var pending = _pendingConfirmation;
                    if (pending is not null)
                    {
                        var arguments = new Dictionary<string, string>(
                            pending.Arguments,
                            StringComparer.OrdinalIgnoreCase)
                        {
                            ["confirmed"] = "true"
                        };
                        _pendingConfirmation = null;
                        OperationalContextStore.RecordState(
                            "EXECUTING",
                            "Boss confirmed the prepared consequential action",
                            pending.Goal);
                        await ExecuteSkillRequestAsync(new SkillRequest(
                            pending.Goal,
                            arguments,
                            _lifetime.Token));
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _operationalVoiceConfirmationGuardV16, 0);
                }
            }
        });
    }

    private void OperationalOutputTranscriptV16(string text)
    {
        OperationalContextStore.RecordAssistant(text);
        Dispatcher.BeginInvoke(() =>
        {
            AddLog($"Jarvis (stable voice): {text}");
            MissionDetailText.Text = text;
        });
    }

    private async Task<string> ExecuteOperationalToolV16Async(
        string functionName,
        JsonElement args,
        CancellationToken cancellationToken)
    {
        string result;
        try
        {
            result = await ExecuteGeminiToolAsyncV12(functionName, args, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            result = $"Tool failed: {exception.Message}";
        }
        OperationalContextStore.RecordTool(functionName, args, result);
        return result;
    }

    private void OperationalVisualFrameReadyV16(string source, byte[] jpeg)
    {
        if (!_operationalVoiceV16.IsRunning) return;
        var now = Environment.TickCount64;
        var previous = Interlocked.Read(ref _lastOperationalVisualTicksV16);
        if (now - previous < 1000) return;
        if (Interlocked.CompareExchange(ref _lastOperationalVisualTicksV16, now, previous) != previous) return;
        _ = _operationalVoiceV16.SendVideoFrameAsync(jpeg, source, _lifetime.Token);
    }
}
