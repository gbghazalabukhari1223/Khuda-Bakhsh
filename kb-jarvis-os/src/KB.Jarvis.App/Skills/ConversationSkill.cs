namespace KB.Jarvis.App.Skills;

public sealed class ConversationSkill : IJarvisSkill
{
    public string Id => "core.conversation";
    public string DisplayName => "Conversation — Greetings, Help and Capability Guidance";

    // This skill is intentionally registered last. It prevents ordinary conversation
    // from being misreported as an execution failure while never pretending that an
    // unsupported computer task was completed.
    public bool CanHandle(SkillRequest request) => true;

    public Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var goal = request.Goal.Trim();
        var normalized = goal.ToLowerInvariant();
        string response;
        SkillStatus status;

        if (IsGreeting(normalized))
        {
            response = "Hello Boss. KB Jarvis is online. You can ask me to open common Windows apps, create and open a Notepad note, inspect or message the currently open WhatsApp chat, list browser tabs, open my workspace, or show my trained skills.";
            status = SkillStatus.Completed;
        }
        else if (IsThanks(normalized))
        {
            response = "You are welcome, Boss. I am ready for the next task.";
            status = SkillStatus.Completed;
        }
        else if (IsHelp(normalized))
        {
            response = "Try commands such as: open calculator; open Chrome; open Notepad; write a Notepad note: \"text\"; list browser tabs; inspect the current WhatsApp chat; draft in the current WhatsApp chat: \"text\"; or send in the current WhatsApp chat: \"text\".";
            status = SkillStatus.Completed;
        }
        else
        {
            response = "I understood the request, but this build does not yet have a verified end-to-end workflow for that exact task. I have not claimed success. Open the Skills or Training module to see the working capabilities and create a reusable workflow.";
            status = SkillStatus.Prepared;
        }

        var steps = new[]
        {
            new SkillStepResult(
                "Classify conversational request",
                true,
                status == SkillStatus.Completed
                    ? "A supported conversational response was selected."
                    : "No executable trained skill matched; an honest capability response was selected instead of reporting a false failure.")
        };

        return Task.FromResult(new SkillResult(Id, status, response, steps));
    }

    private static bool IsGreeting(string text) =>
        text is "hi" or "hello" or "hey" or "salam" or "salaam" or "assalam o alaikum" or "assalamualaikum"
        || text.Contains("how are you")
        || text.Contains("kya haal")
        || text.Contains("kaise ho")
        || text.Contains("kaisay ho");

    private static bool IsThanks(string text) =>
        text.Contains("thank") || text is "thanks" or "shukriya" or "mehrbani" or "meharbani";

    private static bool IsHelp(string text) =>
        text is "help" or "commands" or "capabilities" or "skills"
        || text.Contains("what can you do")
        || text.Contains("kya kar sak")
        || text.Contains("kia kar sak")
        || text.Contains("how to use");
}
