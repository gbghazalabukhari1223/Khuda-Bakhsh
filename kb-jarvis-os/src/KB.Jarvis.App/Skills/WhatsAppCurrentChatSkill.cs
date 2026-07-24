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

    public string Id => "browser.whatsapp.message";
    public string DisplayName => "WhatsApp — Existing Session, Contact Search, Draft and Verified Send";

    public bool CanHandle(SkillRequest request)
    {
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("whatsapp")
               && (goal.Contains("message")
                   || goal.Contains("chat")
                   || goal.Contains("draft")
                   || goal.Contains("send")
                   || goal.Contains("bhej")
                   || goal.Contains("likho")
                   || goal.Contains("type")
                   || goal.Contains("inspect")
                   || request.Arguments.ContainsKey("content")
                   || request.Arguments.ContainsKey("contact"));
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var steps = new List<SkillStepResult>();
        if (!_bridge.IsConnected)
        {
            return new SkillResult(
                Id,
                SkillStatus.Blocked,
                "The KB Jarvis Browser Companion is not connected. Jarvis will not open a duplicate WhatsApp tab. Start Chrome with the existing signed-in tab and reload the Version 13 companion.",
                steps);
        }

        var message = request.Arguments.TryGetValue("content", out var content) ? content.Trim() : string.Empty;
        var contact = request.Arguments.TryGetValue("contact", out var contactValue) ? contactValue.Trim() : string.Empty;
        var confirmed = request.Arguments.TryGetValue("confirmed", out var confirmedValue)
                        && bool.TryParse(confirmedValue, out var confirmation)
                        && confirmation;
        var sendRequested = IsSendRequested(request.Goal);

        if (!string.IsNullOrWhiteSpace(contact))
        {
            var contactResult = await _bridge.ExecuteAsync(
                "whatsapp.contact.open",
                new { contact },
                TimeSpan.FromSeconds(18),
                request.CancellationToken).ConfigureAwait(false);
            steps.Add(new SkillStepResult(
                "Find and verify WhatsApp contact in the existing session",
                contactResult.Success,
                contactResult.Success ? FormatInspectionEvidence(contactResult.Data) : contactResult.Error ?? "Contact search failed."));
            if (!contactResult.Success)
            {
                return new SkillResult(Id, SkillStatus.Blocked, contactResult.Error ?? $"The contact “{contact}” could not be verified.", steps);
            }
        }

        var inspection = await _bridge.ExecuteAsync(
            "whatsapp.current_chat.inspect",
            new { },
            TimeSpan.FromSeconds(10),
            request.CancellationToken).ConfigureAwait(false);

        steps.Add(new SkillStepResult(
            "Inspect existing WhatsApp chat",
            inspection.Success,
            inspection.Success ? FormatInspectionEvidence(inspection.Data) : inspection.Error ?? "Inspection failed."));

        if (!inspection.Success)
        {
            return new SkillResult(Id, SkillStatus.Blocked, inspection.Error ?? "The WhatsApp chat could not be inspected.", steps);
        }

        var verifiedHeader = ReadHeader(inspection.Data);
        if (string.IsNullOrWhiteSpace(message))
        {
            return new SkillResult(
                Id,
                SkillStatus.Completed,
                string.IsNullOrWhiteSpace(verifiedHeader)
                    ? "The existing WhatsApp tab and current message composer were detected successfully."
                    : $"The existing WhatsApp chat was verified: {verifiedHeader}.",
                steps);
        }

        if (sendRequested && !confirmed)
        {
            return new SkillResult(
                Id,
                SkillStatus.Prepared,
                $"The verified WhatsApp chat is ready. Confirm once to send: {message}",
                steps,
                RequiresConfirmation: true,
                ConfirmationPrompt: $"Send this message to {(string.IsNullOrWhiteSpace(verifiedHeader) ? "the verified WhatsApp chat" : verifiedHeader)}?\n\n{message}");
        }

        var operation = sendRequested ? "whatsapp.current_chat.send" : "whatsapp.current_chat.draft";
        var action = await _bridge.ExecuteAsync(
            operation,
            new { message },
            TimeSpan.FromSeconds(20),
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
                    ? "The message was sent and verified in the existing WhatsApp session."
                    : "The exact message was typed and verified as a draft in the existing WhatsApp session.",
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

    private static string? ReadHeader(JsonElement? data) =>
        data is { } element && element.TryGetProperty("chatHeader", out var headerElement)
            ? headerElement.GetString()
            : null;

    private static string FormatInspectionEvidence(JsonElement? data)
    {
        var header = ReadHeader(data);
        return string.IsNullOrWhiteSpace(header)
            ? "A visible WhatsApp chat composer was verified in the existing tab."
            : $"WhatsApp chat header verified: {header}";
    }

    private static string FormatActionEvidence(JsonElement? data, bool sent)
    {
        var header = ReadHeader(data);
        return sent
            ? $"Outgoing message verified{(string.IsNullOrWhiteSpace(header) ? string.Empty : $" in chat: {header}")}."
            : $"Exact draft text verified{(string.IsNullOrWhiteSpace(header) ? string.Empty : $" in chat: {header}")}.";
    }
}