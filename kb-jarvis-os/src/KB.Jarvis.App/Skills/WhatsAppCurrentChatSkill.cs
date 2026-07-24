using System.Text.Json;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App.Skills;

public sealed class WhatsAppCurrentChatSkill : IJarvisSkill
{
    private readonly BrowserBridgeService _bridge;

    public WhatsAppCurrentChatSkill(BrowserBridgeService bridge)
    {
        _bridge = bridge;
    }

    public string Id => "browser.whatsapp.current-chat";
    public string DisplayName => "WhatsApp — Current Chat Draft and Verified Send";

    public bool CanHandle(SkillRequest request)
    {
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("whatsapp")
               && (goal.Contains("current chat")
                   || goal.Contains("open chat")
                   || goal.Contains("is chat")
                   || goal.Contains("iss chat")
                   || goal.Contains("chat mein")
                   || goal.Contains("chat men"));
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var steps = new List<SkillStepResult>();
        if (!_bridge.IsConnected)
        {
            return new SkillResult(
                Id,
                SkillStatus.Blocked,
                "The KB Jarvis Browser Companion is not connected. Start or reload the Version 11 extension; it will reconnect automatically.",
                steps);
        }

        var message = request.Arguments.TryGetValue("content", out var content) ? content.Trim() : string.Empty;
        var confirmed = request.Arguments.TryGetValue("confirmed", out var confirmedValue)
                        && bool.TryParse(confirmedValue, out var confirmation)
                        && confirmation;
        var sendRequested = IsSendRequested(request.Goal);

        var inspection = await _bridge.ExecuteAsync(
            "whatsapp.current_chat.inspect",
            new { },
            TimeSpan.FromSeconds(10),
            request.CancellationToken).ConfigureAwait(false);

        steps.Add(new SkillStepResult(
            "Inspect existing WhatsApp current chat",
            inspection.Success,
            inspection.Success ? FormatInspectionEvidence(inspection.Data) : inspection.Error ?? "Inspection failed."));

        if (!inspection.Success)
        {
            return new SkillResult(Id, SkillStatus.Blocked, inspection.Error ?? "The current WhatsApp chat could not be inspected.", steps);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return new SkillResult(
                Id,
                SkillStatus.Completed,
                "The current WhatsApp chat and message composer were detected successfully.",
                steps);
        }

        if (sendRequested && !confirmed)
        {
            return new SkillResult(
                Id,
                SkillStatus.Prepared,
                $"The current WhatsApp chat is ready. Confirm once to send: {message}",
                steps,
                RequiresConfirmation: true,
                ConfirmationPrompt: $"Send this message to the verified current WhatsApp chat?\n\n{message}");
        }

        var operation = sendRequested ? "whatsapp.current_chat.send" : "whatsapp.current_chat.draft";
        var action = await _bridge.ExecuteAsync(
            operation,
            new { message },
            TimeSpan.FromSeconds(18),
            request.CancellationToken).ConfigureAwait(false);

        steps.Add(new SkillStepResult(
            sendRequested ? "Send and verify outgoing WhatsApp message" : "Type and verify WhatsApp draft",
            action.Success,
            action.Success ? FormatActionEvidence(action.Data, sendRequested) : action.Error ?? "WhatsApp action failed."));

        return action.Success
            ? new SkillResult(
                Id,
                SkillStatus.Completed,
                sendRequested
                    ? "The message was sent and verified in the current WhatsApp chat."
                    : "The exact message was typed and verified as a draft in the current WhatsApp chat.",
                steps)
            : new SkillResult(
                Id,
                SkillStatus.Failed,
                action.Error ?? "The WhatsApp action could not be verified.",
                steps);
    }

    private static bool IsSendRequested(string goal)
    {
        var normalized = goal.ToLowerInvariant();
        return normalized.Contains("send")
               || normalized.Contains("bhej")
               || normalized.Contains("بھج")
               || normalized.Contains("message karo")
               || normalized.Contains("message kr");
    }

    private static string FormatInspectionEvidence(JsonElement? data)
    {
        if (data is not { } element)
        {
            return "WhatsApp composer detected.";
        }

        var header = element.TryGetProperty("chatHeader", out var headerElement)
            ? headerElement.GetString()
            : null;
        return string.IsNullOrWhiteSpace(header)
            ? "A visible current-chat composer was verified."
            : $"Current chat header verified: {header}";
    }

    private static string FormatActionEvidence(JsonElement? data, bool sent)
    {
        if (data is not { } element)
        {
            return sent ? "Outgoing message verified." : "Draft text verified.";
        }

        var header = element.TryGetProperty("chatHeader", out var headerElement)
            ? headerElement.GetString()
            : null;
        return sent
            ? $"Outgoing message verified{(string.IsNullOrWhiteSpace(header) ? string.Empty : $" in chat: {header}")}."
            : $"Exact draft text verified{(string.IsNullOrWhiteSpace(header) ? string.Empty : $" in chat: {header}")}.";
    }
}
