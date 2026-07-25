using System.Text.RegularExpressions;

namespace KB.Jarvis.App.Services;

public sealed class SpeechOutputService
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task SpeakAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text) || SpeechModeCoordinator.LiveVoiceActive)
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (SpeechModeCoordinator.LiveVoiceActive) return;

            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (SpeechModeCoordinator.LiveVoiceActive) return;

                var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (voiceType is null)
                {
                    throw new InvalidOperationException("Windows SAPI voice is unavailable.");
                }

                dynamic voice = Activator.CreateInstance(voiceType)
                    ?? throw new InvalidOperationException("Windows SAPI voice could not be created.");
                try
                {
                    voice.Rate = -1;
                    voice.Volume = 96;
                    foreach (var segment in SplitForSpeech(text))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (SpeechModeCoordinator.LiveVoiceActive) break;
                        voice.Speak(segment, 0);
                    }
                }
                finally
                {
                    try { System.Runtime.InteropServices.Marshal.FinalReleaseComObject(voice); } catch { }
                }
            }, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static IEnumerable<string> SplitForSpeech(string text)
    {
        foreach (var sentence in Regex.Split(text.Trim(), @"(?<=[.!?۔])\s+"))
        {
            var remaining = sentence.Trim();
            while (remaining.Length > 320)
            {
                var split = remaining.LastIndexOf(' ', 320);
                if (split < 80) split = 320;
                yield return remaining[..split].Trim();
                remaining = remaining[split..].Trim();
            }
            if (!string.IsNullOrWhiteSpace(remaining)) yield return remaining;
        }
    }
}
