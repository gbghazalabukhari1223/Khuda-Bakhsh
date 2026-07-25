namespace KB.Jarvis.App.Services;

public static class SpeechModeCoordinator
{
    private static int _liveVoiceActive;

    public static bool LiveVoiceActive => Volatile.Read(ref _liveVoiceActive) == 1;

    public static void SetLiveVoiceActive(bool active) =>
        Interlocked.Exchange(ref _liveVoiceActive, active ? 1 : 0);
}
