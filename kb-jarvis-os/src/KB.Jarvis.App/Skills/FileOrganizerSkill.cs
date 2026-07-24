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
        var sourceValue = Get(request, "source", InferSource(request.Goal));
        var roots = ResolveSourceRoots(sourceValue)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(Directory.Exists)
            .ToArray();
        var destination = ResolveDestination(
            request,
            roots.FirstOrDefault() ?? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        var pattern = NormalizePattern(Get(request, "pattern", InferPattern(request.Goal)));
        var recursive = GetBool(request, "recursive", false);
        var confirmed = GetBool(request, "confirmed", false);
        var mode = Get(request, "mode", "move").Equals("copy", StringComparison.OrdinalIgnoreCase)
            ? "copy"
            : "move";

        if (action is "create_folder" or "mkdir")
        {
            Directory.CreateDirectory(destination);
            var verified = Directory.Exists(destination);
            return new SkillResult(
                Id,
                verified ? SkillStatus.Completed : SkillStatus.Failed,
                verified
                    ? $"Folder created and verified: {destination}"
                    : $"Folder creation could not be verified: {destination}",
                new[] { new SkillStepResult("Create folder", verified, destination) });
        }

        if (roots.Length == 0)
        {
            return new SkillResult(
                Id,
                SkillStatus.Blocked,
                $"No accessible source folder was resolved from: {sourceValue}",
                Array.Empty<SkillStepResult>());
        }

        var files = await Task.Run(
            () => FindFiles(roots, destination, pattern, recursive, request.CancellationToken),
            request.CancellationToken).ConfigureAwait(false);

        if (files.Count == 0)
        {
            return new SkillResult(
                Id,
                SkillStatus.Completed,
                $"No files matching “{pattern}” were found in the selected user locations.",
                new[]
                {
                    new SkillStepResult(
                        "Scan source folders",
                        true,
                        $"Scanned {roots.Length} accessible location(s); no changes were made.")
                });
        }

        if (!confirmed)
        {
            var preview = string.Join(
                Environment.NewLine,
                files.Take(12).Select(path => $"• {Path.GetFileName(path)} — {Path.GetDirectoryName(path)}"));
            return new SkillResult(
                Id,
                SkillStatus.Prepared,
                $"Ready to {mode} {files.Count} matching file(s) from {roots.Length} location(s) into:"
                + $"{Environment.NewLine}{destination}{Environment.NewLine}{Environment.NewLine}{preview}",
                new[]
                {
                    new SkillStepResult(
                        "Build organization plan",
                        true,
                        $"Matched {files.Count} file(s) using {pattern}.")
                },
                RequiresConfirmation: true,
                ConfirmationPrompt: $"Create “{destination}” and {mode} {files.Count} matching file(s) into it?");
        }

        Directory.CreateDirectory(destination);
        var completed = new List<string>();
        var failures = new List<string>();
        foreach (var file in files)
        {
            request.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var target = GetCollisionSafePath(destination, Path.GetFileName(file));
                if (mode == "copy") File.Copy(file, target, overwrite: false);
                else File.Move(file, target);

                if (File.Exists(target) && (mode == "copy" || !File.Exists(file))) completed.Add(target);
                else failures.Add(file);
            }
            catch (Exception exception)
            {
                failures.Add($"{file} — {exception.Message}");
            }
        }

        var success = completed.Count > 0 && failures.Count == 0;
        if (GetBool(request, "open", true) && Directory.Exists(destination))
        {
            Process.Start(new ProcessStartInfo { FileName = destination, UseShellExecute = true });
        }

        var pastTense = mode == "copy" ? "copied" : "moved";
        var summary = failures.Count == 0
            ? $"Created the folder and {pastTense} {completed.Count} file(s), verified at:"
              + $"{Environment.NewLine}{destination}"
            : $"{pastTense} {completed.Count} file(s) to {destination}; "
              + $"{failures.Count} item(s) could not be processed.";
        return new SkillResult(
            Id,
            success ? SkillStatus.Completed : completed.Count > 0 ? SkillStatus.Prepared : SkillStatus.Failed,
            summary,
            new[]
            {
                new SkillStepResult("Create destination folder", Directory.Exists(destination), destination),
                new SkillStepResult($"{mode} matching files", completed.Count > 0, $"Verified {completed.Count} destination file(s)."),
                new SkillStepResult(
                    "Check failures",
                    failures.Count == 0,
                    failures.Count == 0 ? "No failures." : string.Join(" | ", failures.Take(5)))
            });
    }

    private static List<string> FindFiles(
        IReadOnlyCollection<string> roots,
        string destination,
        string pattern,
        bool recursive,
        CancellationToken token)
    {
        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var destinationFull = Path.GetFullPath(destination).TrimEnd(Path.DirectorySeparatorChar)
                              + Path.DirectorySeparatorChar;
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                foreach (var path in Directory.EnumerateFiles(root, pattern, option))
                {
                    token.ThrowIfCancellationRequested();
                    var full = Path.GetFullPath(path);
                    if (full.StartsWith(destinationFull, StringComparison.OrdinalIgnoreCase)) continue;
                    results.Add(full);
                    if (results.Count >= 5000) break;
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
            if (results.Count >= 5000) break;
        }

        return results.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string ResolveDestination(SkillRequest request, string source)
    {
        var requested = Get(request, "destination", string.Empty);
        if (!string.IsNullOrWhiteSpace(requested)) return ResolvePath(requested);

        var inferredName = InferFolderName(request.Goal);
        var name = Get(request, "folder_name", inferredName ?? "Organized Notepad Files");
        var baseFolder = IsPcAlias(Get(request, "source", string.Empty))
            ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
            : source;
        return Path.Combine(baseFolder, SanitizeFolderName(name));
    }

    private static IEnumerable<string> ResolveSourceRoots(string source)
    {
        if (IsPcAlias(source))
        {
            yield return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            yield return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            yield break;
        }

        yield return ResolvePath(source);
    }

    private static bool IsPcAlias(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "pc" or "computer" or "my pc" or "standard" or "user folders"
            or "meray pc" or "mere pc";
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

    private static string InferSource(string goal)
    {
        var lower = goal.ToLowerInvariant();
        if (lower.Contains("desktop")) return "Desktop";
        if (lower.Contains("documents")) return "Documents";
        if (lower.Contains("downloads")) return "Downloads";
        if (lower.Contains("pc") || lower.Contains("computer")) return "PC";
        return "Desktop";
    }

    private static string InferPattern(string goal)
    {
        var lower = goal.ToLowerInvariant();
        if (lower.Contains("notepad") || lower.Contains("text file") || lower.Contains("txt")) return "*.txt";
        if (lower.Contains("image") || lower.Contains("photo")) return "*.jpg";
        if (lower.Contains("pdf")) return "*.pdf";
        if (lower.Contains("html")) return "*.html";
        if (lower.Contains("css")) return "*.css";
        if (lower.Contains("javascript") || lower.Contains(" js ")) return "*.js";
        return "*.*";
    }

    private static string? InferFolderName(string goal)
    {
        var quoted = Regex.Match(goal, "folder\\s+(?:named|name)?\\s*[\\\"'“”]([^\\\"'“”]+)[\\\"'“”]", RegexOptions.IgnoreCase);
        if (quoted.Success) return quoted.Groups[1].Value.Trim();

        var marker = Regex.Match(goal, "folder\\s+(?:named|name|ka naam|naam)\\s+([a-zA-Z0-9 _-]{2,60})", RegexOptions.IgnoreCase);
        if (!marker.Success) return null;
        var value = marker.Groups[1].Value;
        var stopWords = new[] { " mein ", " men ", " main ", " me ", " and ", " aur ", " move ", " organize ", " banao ", " bana ", " create " };
        var cut = stopWords
            .Select(word => value.IndexOf(word, StringComparison.OrdinalIgnoreCase))
            .Where(index => index >= 0)
            .DefaultIfEmpty(value.Length)
            .Min();
        return value[..cut].Trim();
    }

    private static string NormalizePattern(string pattern)
    {
        pattern = pattern.Trim();
        if (string.IsNullOrWhiteSpace(pattern)) return "*.*";
        if (pattern.StartsWith('.')) return $"*{pattern}";
        if (!pattern.Contains('*') && !pattern.Contains('?') && Regex.IsMatch(pattern, "^[a-zA-Z0-9]+$"))
            return $"*.{pattern}";
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