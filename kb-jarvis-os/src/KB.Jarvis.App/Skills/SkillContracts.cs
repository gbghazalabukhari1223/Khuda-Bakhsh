namespace KB.Jarvis.App.Skills;

public enum SkillStatus
{
    Completed,
    Prepared,
    Blocked,
    Failed
}

public sealed record SkillRequest(
    string Goal,
    IReadOnlyDictionary<string, string> Arguments,
    CancellationToken CancellationToken);

public sealed record SkillStepResult(
    string Step,
    bool Success,
    string Evidence,
    string? RecoveryMethod = null);

public sealed record SkillResult(
    string SkillId,
    SkillStatus Status,
    string Summary,
    IReadOnlyList<SkillStepResult> Steps,
    bool RequiresConfirmation = false,
    string? ConfirmationPrompt = null);

public interface IJarvisSkill
{
    string Id { get; }
    string DisplayName { get; }
    bool CanHandle(SkillRequest request);
    Task<SkillResult> ExecuteAsync(SkillRequest request);
}
