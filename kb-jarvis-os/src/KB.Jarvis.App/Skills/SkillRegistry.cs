using KB.Jarvis.App.Core;

namespace KB.Jarvis.App.Skills;

public sealed class SkillRegistry
{
    private readonly List<IJarvisSkill> _skills = new();

    public IReadOnlyList<IJarvisSkill> Skills => _skills;

    public void Register(IJarvisSkill skill)
    {
        ArgumentNullException.ThrowIfNull(skill);
        if (_skills.Any(existing => string.Equals(existing.Id, skill.Id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"A skill with id '{skill.Id}' is already registered.");
        }

        _skills.Add(skill);
        JarvisLog.Info($"Registered skill: {skill.Id} ({skill.DisplayName})");
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var skill = _skills.FirstOrDefault(candidate => candidate.CanHandle(request));
        if (skill is null)
        {
            return new SkillResult(
                "unmatched-goal",
                SkillStatus.Blocked,
                "No trained deterministic skill matched this goal yet.",
                Array.Empty<SkillStepResult>());
        }

        JarvisLog.Info($"Executing skill {skill.Id} for goal: {request.Goal}");
        try
        {
            return await skill.ExecuteAsync(request).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return new SkillResult(
                skill.Id,
                SkillStatus.Blocked,
                "The mission was cancelled before completion.",
                Array.Empty<SkillStepResult>());
        }
        catch (Exception exception)
        {
            JarvisLog.Error($"Skill {skill.Id} failed", exception);
            return new SkillResult(
                skill.Id,
                SkillStatus.Failed,
                exception.Message,
                Array.Empty<SkillStepResult>());
        }
    }
}
