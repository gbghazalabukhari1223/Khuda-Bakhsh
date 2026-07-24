using System.Diagnostics;
using System.Text.RegularExpressions;

namespace KB.Jarvis.App.Skills;

public sealed class FileOrganizerSkill : IJarvisSkill
{
    public string Id => "files.organize";
    public string DisplayName => "Files — Create Folder, Organize, Move and Verify";

    public bool CanHandle(SkillRequest request)
    {
        if (request.Arguments.ContainsKey("action")) return true;
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("organize")
               || goal.Contains("move file")
               || goal.Contains("move all")
               || goal.Contains("folder banao")
               || goal.Contains("folder bana")
               || goal.Contains("folder mein")
               || goal.Contains("folder men")
               || goal.Contains("files ko")
               || goal.Contains("فولڈر");
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var action = Get(request, "action", "organize").ToLowerInvariant();
        var source = ResolvePath(Get(request, "source", Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)));
        var destination = ResolveDestination(request, source);
        var pattern = NormalizePattern(Get(request, "pattern", InferPattern(request.Goal)));
        var recursive = GetBool(request, "recursive", false);
        var confirmed = GetBool(request, "confirmed", false);
        var mode = Get(request, "mode", "move").ToLowerInvariant();

        if (action is "create_folder" or "mkdir")
        {
            Directory.CreateDirectory(destination);
            var verified = Directory.Exists(destination);
            return new SkillResult(
                Id,
                verified ? SkillStatus.Completed : SkillStatus.Failed,
                verified ? $"Folder created and verified: {destination}" : $"Folder creation could not be verified: {destination}",
                new[] { new SkillStepResult("Create folder", verified, destination) });
        }

        if (!Directory.Exists(source))
        {
            return new SkillResult(Id, SkillStatus.Blocked, $"Source folder does not exist: {source}", Array.Empty<SkillStepResult>());
        }

        var files = await Task.Run(
            () => FindFiles(source, destination, pattern, recursive, request.CancellationToken),
            request.CancellationToken).ConfigureAwait(false);

        if (files.Count == 0)
        {
            return new SkillResult(
                Id,
                SkillStatus.Completed,
                $"No files matching “{pattern}” were found in {source}.",
                new[] { new SkillStepResult("Scan source folder", true, "No matching files found; no changes were made.") });
        }

        if (!confirmed)
        {
            var preview = string.Join(Environment.NewLine, files.Take(12).Select(path => $"• {Path.GetFileName(path)}"));
            return new SkillResult(
                Id,
                SkillStatus.Prepared,
                $"Ready to {mode} {files.Count} matching file(s) into:\n{destination}\n\n{preview}",
                new[] { new SkillStepResult("Build organization plan", true, $"Matched {files.Count} file(s) using {pattern}.") },
                RequiresConfirmation: true,
                ConfirmationPrompt: $"Create “{destination}” and {mode} {files.Count} matching file(s) into it?");
        }

        Directory.CreateDirectory(destination);
        var moved = new List<string>();
        var failures = new List<string>();
        foreach (var file in files)
        {
            request.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var target = GetCollisionSafePath(destination, Path.GetFileName(file));
                if (mode == "copy") File.Copy(file, target, overwrite: false);
                else File.Move(file, target);
                if (File.Exists(target) && (mode == "copy" || !File.Exists(file))) moved.Add(target);
                else failures.Add(file);
            }
            catch (Exception exception)
            {
                failures.Add($"{file} — {exception.Message}");
            }
        }

        var success = moved.Count > 0 && failures.Count == 0;
        if (GetBool(request, "open", true) && Directory.Exists(destination))
        {
            Process.Start(new ProcessStartInfo { FileName = destination, UseShellExecute = true });
        }

        var summary = failures.Count == 0
            ? $"Created the folder and {mode}d {moved.Count} file(s), verified at:\n{destination}"
            : $"{mode}d {moved.Count} file(s) to {destination}; {failures.Count} item(s) could not be processed.";
        return new SkillResult(
            Id,
            success ? SkillStatus.Completed : moved.Count > 0 ? SkillStatus.Prepared : SkillStatus.Failed,
            summary,
            new[]
            {
                new SkillStepResult("Create destination folder", Directory.Exists(destination), destination),
                new SkillStepResult($"{mode} matching files", moved.Count > 0, $"Verified {moved.Count} destination file(s)."),
                new SkillStepResult("Check failures", failures.Count == 0, failures.Count == 0 ? "No failures." : string.Join(" | ", failures.Take(5)))
            });
    }

    private static List<string> FindFiles(string source, string destination, string pattern, bool recursive, CancellationToken token)
    {
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var destinationFull = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        try
        {
            return Directory.EnumerateFiles(source, pattern, option)
                .Where(path => !Path.GetFullPath(path).StartsWith(destinationFull, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Take(5000)
                .Select(path => { token.ThrowIfCancellationRequested(); return path; })
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            return new List<string>();
        }
    }

    private static string ResolveDestination(SkillRequest request, string source)
    {
        var requested = Get(request, "destination", string.Empty);
        if (!string.IsNullOrWhiteSpace(requested)) return ResolvePath(requested);
        var name = Get(request, "folder_name", "Organized Notepad Files");
        return Path.Combine(source, SanitizeFolderName(name));
    }

    private static string ResolvePath(string path)
    {
        var expanded = Environment.ExpandEnvironmentVariables(path.Trim().Trim('"'));
        if (expanded.Equals("desktop", StringComparison.OrdinalIgnoreCase))
            expanded = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        else if (expanded.Equals("documents", StringComparison.OrdinalIgnoreCase))
            expanded = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        else if (expanded.Equals("downloads", StringComparison.OrdinalIgnoreCase))
            expanded = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        return Path.GetFullPath(expanded);
    }

    private static string InferPattern(string goal)
    {
        var lower = goal.ToLowerInvariant();
        if (lower.Contains("notepad") || lower.Contains("text file") || lower.Contains("txt")) return "*.txt";
        if (lower.Contains("image") || lower.Contains("photo")) return "*.jpg";
        if (lower.Contains("pdf")) return "*.pdf";
        if (lower.Contains("html")) return "*.html";
        return "*.*";
    }

    private static string NormalizePattern(string pattern)
    {
        pattern = pattern.Trim();
        if (string.IsNullOrWhiteSpace(pattern)) return "*.*";
        if (pattern.StartsWith('.')) return $"*{pattern}";
        if (!pattern.Contains('*') && !pattern.Contains('?') && Regex.IsMatch(pattern, "^[a-zA-Z0-9]+$")) return $"*.{pattern}";
        return pattern;
    }

    private static string GetCollisionSafePath(string folder, string fileName)
    {
        var target = Path.Combine(folder, fileName);
        if (!File.Exists(target)) return target;
        var stem = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        for (var index = 2; index < 10000; index++)
        {
            target = Path.Combine(folder, $"{stem} ({index}){extension}");
            if (!File.Exists(target)) return target;
        }
        throw new IOException($"Could not create a collision-safe name for {fileName}.");
    }

    private static string SanitizeFolderName(string name)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars()) name = name.Replace(invalid, '-');
        return string.IsNullOrWhiteSpace(name) ? "Organized Files" : name.Trim();
    }

    private static string Get(SkillRequest request, string key, string fallback) =>
        request.Arguments.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;

    private static bool GetBool(SkillRequest request, string key, bool fallback) =>
        request.Arguments.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;
}