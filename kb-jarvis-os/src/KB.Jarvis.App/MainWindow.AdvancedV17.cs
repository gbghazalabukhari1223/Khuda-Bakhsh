using System.Windows.Media;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App;

public partial class MainWindow
{
    private int _advancedV17Initialized;

    public void InitializeAdvancedV17()
    {
        if (Interlocked.Exchange(ref _advancedV17Initialized, 1) == 1) return;

        _operationalVoiceV16.HealthUpdated += AdvancedVoiceHealthUpdatedV17;
        _operationalVoiceV16.Diagnostic += message =>
            Dispatcher.BeginInvoke(() =>
            {
                if (message.Contains("barge-in", StringComparison.OrdinalIgnoreCase))
                {
                    CoreStateText.Text = "BARGE-IN · LISTENING";
                    CoreStateText.Foreground = Brushes.Orange;
                }
            });

        Title = "KB Jarvis OS 17 — Advanced Iron HUD";
        OperationalContextStore.RecordState(
            "READY",
            "KB Jarvis OS 17 advanced voice reactor and Iron HUD loaded",
            expectedPostcondition: "Real operational tasks execute through preserved skills and independently verified workflows");

        AddLog("V17 ADVANCED · Previous Jarvis capabilities preserved.");
        AddLog("V17 VOICE · Continuous WASAPI output, adaptive jitter buffer, 80 ms microphone batching, barge-in and voice-priority streaming loaded.");
        AddLog("V17 HUD · Cinematic Iron interface, voice reactor telemetry and operational command deck loaded.");

        SetMissionState(
            "KB Jarvis OS 17 is ready, Boss.",
            "Advanced voice and the Iron HUD are online. Existing browser, WordPress, WhatsApp, website, file, visual, desktop and multi-task capabilities remain available.",
            "V17 ADVANCED READY",
            Brushes.LightGreen);
    }

    private void AdvancedVoiceHealthUpdatedV17(VoiceHealthSnapshot health)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (VoiceModeText is null) return;

            VoiceModeText.Text = health.OutputMode.ToUpperInvariant();
            VoiceBufferText.Text = $"{health.BufferedMilliseconds} ms";
            VoiceTargetText.Text = $"{health.TargetPrebufferMilliseconds} ms";
            VoiceUnderrunText.Text = health.PlaybackUnderruns.ToString();
            VoiceMicQueueText.Text = health.QueuedMicrophonePackets.ToString();
            VoiceBargeInText.Text = health.BargeIns.ToString();

            var healthy = health.BufferedMilliseconds <= 1800
                          && health.QueuedMicrophonePackets < 45;
            VoiceBufferText.Foreground = healthy ? Brushes.LightGreen : Brushes.Orange;
            VoiceMicQueueText.Foreground = health.QueuedMicrophonePackets < 45
                ? Brushes.LightGreen
                : Brushes.OrangeRed;

            if (_operationalVoiceV16.IsRunning)
            {
                CoreStateText.Text = health.BufferedMilliseconds > 1800
                    ? "VOICE LATENCY · RECOVERING"
                    : "VOICE REACTOR · STABLE";
                CoreStateText.Foreground = health.BufferedMilliseconds > 1800
                    ? Brushes.Orange
                    : Brushes.LightGreen;
            }
        });
    }
}
