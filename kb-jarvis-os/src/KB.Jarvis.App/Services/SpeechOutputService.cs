namespace KB.Jarvis.App.Services;

public sealed class SpeechOutputService
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task SpeakAsync(string text, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var voiceType = Type.GetTypeFromProgID("SAPI.SpVoice");
                if (voiceType is null)
                {
                    throw new InvalidOperationException("Windows SAPI voice is unavailable.");
                }

                dynamic voice = Activator.CreateInstance(voiceType)
                    ?? throw new InvalidOperationException("Windows SAPI voice could not be created.");
                try
                {
                    voice.Rate = 0;
                    voice.Volume = 100;
                    voice.Speak(text, 0);
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
}