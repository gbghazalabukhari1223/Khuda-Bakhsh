using System.Diagnostics;
using System.IO;
using KB.Jarvis.App.Native;

namespace KB.Jarvis.App.Skills;

public sealed class NotepadWriteSkill : IJarvisSkill
{
    public string Id => "desktop.notepad.write-save";
    public string DisplayName => "Notepad — Write, Save and Verify";

    public bool CanHandle(SkillRequest request)
    {
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("notepad") || goal.Contains("note pad") || goal.Contains("نوٹ پیڈ");
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var steps = new List<SkillStepResult>();
        var content = request.Arguments.TryGetValue("content", out var requestedContent)
            ? requestedContent
            : request.Goal;

        var path = request.Arguments.TryGetValue("path", out var requestedPath)
            ? Environment.ExpandEnvironmentVariables(requestedPath)
            : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                $"KB-Jarvis-Note-{DateTime.Now:yyyyMMdd-HHmmss}.txt");

        path = Path.GetFullPath(path);
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The note path has no parent directory."));

        await File.WriteAllTextAsync(path, content, request.CancellationToken).ConfigureAwait(false);
        var diskText = await File.ReadAllTextAsync(path, request.CancellationToken).ConfigureAwait(false);
        var fileVerified = string.Equals(diskText, content, StringComparison.Ordinal);
        steps.Add(new SkillStepResult(
            "Write note atomically to disk",
            fileVerified,
            fileVerified ? $"Verified exact content at {path}" : "The file content did not match the requested text."));

        if (!fileVerified)
        {
            return new SkillResult(Id, SkillStatus.Failed, "The note could not be verified on disk.", steps);
        }

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = "notepad.exe",
            Arguments = $"\"{path}\"",
            UseShellExecute = true
        });

        if (process is null)
        {
            steps.Add(new SkillStepResult("Open Notepad", false, "Windows did not return a Notepad process."));
            return new SkillResult(Id, SkillStatus.Failed, "The file was saved, but Notepad could not be opened.", steps);
        }

        var activated = await NativeInput.ActivateProcessWindowAsync(
            process,
            TimeSpan.FromSeconds(8),
            request.CancellationToken).ConfigureAwait(false);

        steps.Add(new SkillStepResult(
            "Open and activate Notepad",
            activated,
            activated
                ? $"Notepad opened with the verified file: {path}"
                : $"The file was saved at {path}, but the Notepad window could not be brought to the foreground.",
            activated ? null : "The saved file remains available and can be reopened without data loss."));

        return new SkillResult(
            Id,
            activated ? SkillStatus.Completed : SkillStatus.Prepared,
            activated
                ? $"The note was saved, verified and opened in Notepad: {path}"
                : $"The note was saved and verified, but Notepad focus was blocked: {path}",
            steps);
    }
}
