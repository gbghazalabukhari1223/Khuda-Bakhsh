using System.Text.Json;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App.Skills;

public sealed class GeminiAgentSkill : IJarvisSkill
{
    private readonly JarvisSettingsStore _settingsStore;
    private readonly GeminiTextAgentService _agent;
    private readonly SpeechOutputService _speech;
    private readonly Func<string, JsonElement, CancellationToken, Task<string>> _toolExecutor;

    public GeminiAgentSkill(
        JarvisSettingsStore settingsStore,
        GeminiTextAgentService agent,
        SpeechOutputService speech,
        Func<string, JsonElement, CancellationToken, Task<string>> toolExecutor)
    {
        _settingsStore = settingsStore;
        _agent = agent;
        _speech = speech;
        _toolExecutor = toolExecutor;
    }

    public string Id => "core.gemini-agent";
    public string DisplayName => "Gemini Agent — Conversation, Planning and Tool Calling";
    public bool CanHandle(SkillRequest request) => true;

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var settings = _settingsStore.Load();
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return new SkillResult(
                Id,
                SkillStatus.Prepared,
                "Gemini intelligence is not configured yet. Open Jarvis Settings, save the API key, then start voice or send the command again.",
                new[]
                {
                    new SkillStepResult("Check Gemini configuration", false, "No encrypted API key was found.", "Open the native Settings window.")
                });
        }

        var reply = await _agent.SendAsync(
            request.Goal,
            settings,
            _toolExecutor,
            request.CancellationToken).ConfigureAwait(false);

        if (settings.SpeakTextReplies)
        {
            try
            {
                await _speech.SpeakAsync(reply, request.CancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // A spoken fallback failure must not discard the verified text response.
            }
        }

        return new SkillResult(
            Id,
            SkillStatus.Completed,
            reply,
            new[]
            {
                new SkillStepResult(
                    "Process goal through Gemini agent",
                    true,
                    "The model response was generated with the local Jarvis tool catalogue enabled.")
            });
    }
}