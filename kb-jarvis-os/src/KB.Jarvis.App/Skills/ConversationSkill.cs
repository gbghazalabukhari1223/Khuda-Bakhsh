namespace KB.Jarvis.App.Skills;

public sealed class ConversationSkill : IJarvisSkill
{
    public string Id => "core.offline-help";
    public string DisplayName => "Offline Help — Local Commands and Diagnostics";

    public bool CanHandle(SkillRequest request)
    {
        var normalized = request.Goal.Trim().ToLowerInvariant();
        return normalized.Contains("offline help")
               || normalized.Contains("local commands")
               || normalized.Contains("without internet")
               || normalized.Contains("bina internet");
    }

    public Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        const string response = "Offline commands include: open calculator; open Chrome; open Notepad; write a Notepad note: \"text\"; list browser tabs; inspect the current WhatsApp chat; draft in the current WhatsApp chat: \"text\"; and send in the current WhatsApp chat: \"text\". Configure Gemini in Settings for natural conversation, voice and AI planning.";
        return Task.FromResult(new SkillResult(
            Id,
            SkillStatus.Completed,
            response,
            new[]
            {
                new SkillStepResult("Load offline command guide", true, "Local help returned without using an external model.")
            }));
    }
}