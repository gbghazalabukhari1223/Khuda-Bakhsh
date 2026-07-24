using System.Diagnostics;

namespace KB.Jarvis.App.Skills;

public sealed class FileSearchSkill : IJarvisSkill
{
    public string Id => "files.search";
    public string DisplayName => "Files — Search, Rank and Open Local Results";

    public bool CanHandle(SkillRequest request)
    {
        if (request.Arguments.ContainsKey("query"))
        {
            return request.Goal.Contains("file", StringComparison.OrdinalIgnoreCase)
                   || request.Goal.Contains("folder", StringComparison.OrdinalIgnoreCase);
        }

        var goal = request.Goal.ToLowerInvariant();
        return (goal.Contains("search") || goal.Contains("find") || goal.Contains("dhoond") || goal.Contains("talash"))
               && (goal.Contains("file") || goal.Contains("folder") || goal.Contains("document") || goal.Contains("pc"));
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var query = request.Arguments.TryGetValue("query", out var supplied)
            ? supplied.Trim()
            : ExtractQuery(request.Goal);
        if (string.IsNullOrWhiteSpace(query))
        {
            return new SkillResult(Id, SkillStatus.Blocked, "A file or folder search term is required.", Array.Empty<SkillStepResult>());
        }

        var openRequested = request.Arguments.TryGetValue("open", out var openRaw)
                            && bool.TryParse(openRaw, out var open)
                            && open;
        var kind = request.Arguments.TryGetValue("kind", out var kindRaw) ? kindRaw.ToLowerInvariant() : "any";
        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var roots = GetSearchRoots().Distinct(StringComparer.OrdinalIgnoreCase).Where(Directory.Exists).ToArray();
        var matches = await Task.Run(
            () => Search(roots, tokens, kind, request.CancellationToken),
            request.CancellationToken).ConfigureAwait(false);

        if (matches.Count == 0)
        {
            return new SkillResult(
                Id,
                SkillStatus.Completed,
                $"No matching files or folders were found for “{query}” in the standard user locations.",
                new[] { new SkillStepResult("Search local files and folders", true, $"Scanned {roots.Length} standard search roots; no matches found.") });
        }

        var top = matches.Take(15).ToArray();
        string? openedPath = null;
        if (openRequested)
        {
            openedPath = top[0].Path;
            Process.Start(new ProcessStartInfo { FileName = openedPath, UseShellExecute = true });
        }

        var listing = string.Join(Environment.NewLine, top.Select((match, index) =>
            $"{index + 1}. {(match.IsDirectory ? "[Folder]" : "[File]")} {match.Path}"));
        var summary = openRequested
            ? $"Found {matches.Count} result(s) for “{query}” and opened the best match:\n{openedPath}\n\nTop matches:\n{listing}"
            : $"Found {matches.Count} result(s) for “{query}”:\n{listing}";

        return new SkillResult(
            Id,
            SkillStatus.Completed,
            summary,
            new[]
            {
                new SkillStepResult("Search local files and folders", true, $"Ranked {matches.Count} matching item(s)."),
                new SkillStepResult("Open best result", !openRequested || openedPath is not null, openRequested ? $"Opened {openedPath}" : "Opening was not requested.")
            });
    }

    private static List<SearchMatch> Search(
        IReadOnlyCollection<string> roots,
        IReadOnlyList<string> tokens,
        string kind,
        CancellationToken cancellationToken)
    {
        const int maximumVisited = 40000;
        var visited = 0;
        var matches = new List<SearchMatch>();
        var queue = new Queue<string>(roots);

        while (queue.Count > 0 && visited < maximumVisited)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var folder = queue.Dequeue();
            IEnumerable<string> entries;
            try
            {
                entries = Directory.EnumerateFileSystemEntries(folder);
            }
            catch
            {
                continue;
            }

            foreach (var entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (++visited > maximumVisited) break;
                bool isDirectory;
                try
                {
                    isDirectory = Directory.Exists(entry);
                }
                catch
                {
                    continue;
                }

                var name = Path.GetFileName(entry);
                var normalized = name.ToLowerInvariant();
                var score = Score(normalized, tokens);
                var kindMatches = kind switch
                {
                    "file" => !isDirectory,
                    "folder" or "directory" => isDirectory,
                    _ => true
                };
                if (kindMatches && score > 0)
                {
                    matches.Add(new SearchMatch(entry, isDirectory, score));
                }

                if (isDirectory)
                {
                    try
                    {
                        var attributes = File.GetAttributes(entry);
                        if (!attributes.HasFlag(FileAttributes.ReparsePoint)
                            && !attributes.HasFlag(FileAttributes.System))
                        {
                            queue.Enqueue(entry);
                        }
                    }
                    catch { }
                }
            }
        }

        return matches
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Path.Length)
            .Take(100)
            .ToList();
    }

    private static int Score(string name, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0) return 0;
        var score = 0;
        foreach (var token in tokens)
        {
            var lowered = token.ToLowerInvariant();
            if (name.Equals(lowered, StringComparison.OrdinalIgnoreCase)) score += 100;
            else if (name.StartsWith(lowered, StringComparison.OrdinalIgnoreCase)) score += 55;
            else if (name.Contains(lowered, StringComparison.OrdinalIgnoreCase)) score += 30;
            else return 0;
        }
        return score;
    }

    private static IEnumerable<string> GetSearchRoots()
    {
        yield return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        yield return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        yield return Path.Combine(profile, "Downloads");
    }

    private static string ExtractQuery(string goal)
    {
        var wordsToRemove = new[]
        {
            "search", "find", "look", "for", "file", "files", "folder", "folders", "document", "documents",
            "on", "in", "my", "pc", "computer", "dhoond", "dhoondo", "talash", "karo", "kro", "please"
        };
        var result = goal;
        foreach (var word in wordsToRemove)
        {
            result = System.Text.RegularExpressions.Regex.Replace(result, $@"\b{System.Text.RegularExpressions.Regex.Escape(word)}\b", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return string.Join(' ', result.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private sealed record SearchMatch(string Path, bool IsDirectory, int Score);
}