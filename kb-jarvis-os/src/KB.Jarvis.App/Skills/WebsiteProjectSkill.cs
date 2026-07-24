using System.Diagnostics;
using System.Text;

namespace KB.Jarvis.App.Skills;

public sealed class WebsiteProjectSkill : IJarvisSkill
{
    public string Id => "website.project";
    public string DisplayName => "Website Studio — Create and Edit HTML, CSS, JavaScript and PHP";

    public bool CanHandle(SkillRequest request)
    {
        if (request.Arguments.ContainsKey("action")) return true;
        var goal = request.Goal.ToLowerInvariant();
        return goal.Contains("website")
               || goal.Contains("web page")
               || goal.Contains("html")
               || goal.Contains("css")
               || goal.Contains("javascript")
               || goal.Contains("java script")
               || goal.Contains("php")
               || goal.Contains("landing page");
    }

    public async Task<SkillResult> ExecuteAsync(SkillRequest request)
    {
        var action = Get(request, "action", "list_files").ToLowerInvariant();
        var projectRoot = ResolveProjectRoot(Get(request, "project", "Boss Website"));
        Directory.CreateDirectory(projectRoot);

        return action switch
        {
            "create_project" => await CreateProjectAsync(projectRoot, request).ConfigureAwait(false),
            "create_page" => await CreatePageAsync(projectRoot, request).ConfigureAwait(false),
            "write_file" or "update_file" => await WriteFileAsync(projectRoot, request).ConfigureAwait(false),
            "search_replace" => await SearchReplaceAsync(projectRoot, request).ConfigureAwait(false),
            "list_files" => ListFiles(projectRoot),
            "validate" => ValidateProject(projectRoot),
            "open" or "open_project" => OpenProject(projectRoot),
            _ => new SkillResult(Id, SkillStatus.Blocked, $"Unsupported website action: {action}", Array.Empty<SkillStepResult>())
        };
    }

    private async Task<SkillResult> CreateProjectAsync(string root, SkillRequest request)
    {
        var title = Get(request, "title", Path.GetFileName(root));
        Directory.CreateDirectory(Path.Combine(root, "css"));
        Directory.CreateDirectory(Path.Combine(root, "js"));
        Directory.CreateDirectory(Path.Combine(root, "images"));
        Directory.CreateDirectory(Path.Combine(root, "pages"));

        var indexPath = Path.Combine(root, "index.html");
        var cssPath = Path.Combine(root, "css", "style.css");
        var jsPath = Path.Combine(root, "js", "app.js");
        if (!File.Exists(indexPath))
        {
            await File.WriteAllTextAsync(indexPath, DefaultHtml(title), Encoding.UTF8, request.CancellationToken).ConfigureAwait(false);
        }
        if (!File.Exists(cssPath))
        {
            await File.WriteAllTextAsync(cssPath, DefaultCss(), Encoding.UTF8, request.CancellationToken).ConfigureAwait(false);
        }
        if (!File.Exists(jsPath))
        {
            await File.WriteAllTextAsync(jsPath, DefaultJavaScript(), Encoding.UTF8, request.CancellationToken).ConfigureAwait(false);
        }

        var verified = File.Exists(indexPath) && File.Exists(cssPath) && File.Exists(jsPath);
        if (GetBool(request, "open", true)) OpenFolder(root);
        return new SkillResult(
            Id,
            verified ? SkillStatus.Completed : SkillStatus.Failed,
            verified ? $"Website project created and verified: {root}" : "Website project files could not be verified.",
            new[]
            {
                new SkillStepResult("Create website folders", Directory.Exists(Path.Combine(root, "css")) && Directory.Exists(Path.Combine(root, "js")), root),
                new SkillStepResult("Create index.html", File.Exists(indexPath), indexPath),
                new SkillStepResult("Create CSS and JavaScript", File.Exists(cssPath) && File.Exists(jsPath), $"{cssPath} | {jsPath}")
            });
    }

    private async Task<SkillResult> CreatePageAsync(string root, SkillRequest request)
    {
        var requested = Get(request, "path", Get(request, "slug", "new-page"));
        if (!Path.HasExtension(requested)) requested += ".html";
        if (!requested.Contains(Path.DirectorySeparatorChar) && !requested.Contains(Path.AltDirectorySeparatorChar))
            requested = Path.Combine("pages", requested);
        var path = ResolveProjectFile(root, requested);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var title = Get(request, "title", Humanize(Path.GetFileNameWithoutExtension(path)));
        var content = Get(request, "content", string.Empty);
        var html = string.IsNullOrWhiteSpace(content) ? DefaultPageHtml(title, RelativeAssetPrefix(root, path)) : content;
        await WriteWithBackupAsync(path, html, request.CancellationToken).ConfigureAwait(false);
        var verified = File.Exists(path) && string.Equals(await File.ReadAllTextAsync(path, request.CancellationToken).ConfigureAwait(false), html, StringComparison.Ordinal);
        return new SkillResult(
            Id,
            verified ? SkillStatus.Completed : SkillStatus.Failed,
            verified ? $"Web page created and verified: {path}" : $"The page could not be verified: {path}",
            new[] { new SkillStepResult("Create page", verified, path) });
    }

    private async Task<SkillResult> WriteFileAsync(string root, SkillRequest request)
    {
        var relative = Get(request, "path", string.Empty);
        var content = Get(request, "content", string.Empty);
        if (string.IsNullOrWhiteSpace(relative))
            return new SkillResult(Id, SkillStatus.Blocked, "A website file path is required.", Array.Empty<SkillStepResult>());
        var path = ResolveProjectFile(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var existed = File.Exists(path);
        await WriteWithBackupAsync(path, content, request.CancellationToken).ConfigureAwait(false);
        var disk = await File.ReadAllTextAsync(path, request.CancellationToken).ConfigureAwait(false);
        var verified = string.Equals(disk, content, StringComparison.Ordinal);
        return new SkillResult(
            Id,
            verified ? SkillStatus.Completed : SkillStatus.Failed,
            verified ? $"Website file {(existed ? "updated" : "created")} and verified: {path}" : $"File content verification failed: {path}",
            new[]
            {
                new SkillStepResult(existed ? "Backup existing file" : "Prepare new file", !existed || Directory.EnumerateFiles(Path.GetDirectoryName(path)!, Path.GetFileName(path) + ".kb-backup-*").Any(), path),
                new SkillStepResult("Write exact content", verified, path)
            });
    }

    private async Task<SkillResult> SearchReplaceAsync(string root, SkillRequest request)
    {
        var relative = Get(request, "path", string.Empty);
        var search = Get(request, "search", string.Empty);
        var replace = Get(request, "replace", string.Empty);
        if (string.IsNullOrWhiteSpace(relative) || string.IsNullOrEmpty(search))
            return new SkillResult(Id, SkillStatus.Blocked, "File path and search text are required.", Array.Empty<SkillStepResult>());
        var path = ResolveProjectFile(root, relative);
        if (!File.Exists(path))
            return new SkillResult(Id, SkillStatus.Blocked, $"Website file does not exist: {path}", Array.Empty<SkillStepResult>());
        var original = await File.ReadAllTextAsync(path, request.CancellationToken).ConfigureAwait(false);
        var count = CountOccurrences(original, search);
        if (count == 0)
            return new SkillResult(Id, SkillStatus.Completed, $"Search text was not found in {path}; no changes were made.", new[] { new SkillStepResult("Find requested text", true, "0 matches") });
        var updated = original.Replace(search, replace, StringComparison.Ordinal);
        await WriteWithBackupAsync(path, updated, request.CancellationToken).ConfigureAwait(false);
        var verified = string.Equals(await File.ReadAllTextAsync(path, request.CancellationToken).ConfigureAwait(false), updated, StringComparison.Ordinal);
        return new SkillResult(
            Id,
            verified ? SkillStatus.Completed : SkillStatus.Failed,
            verified ? $"Updated {count} occurrence(s) and verified: {path}" : $"Updated file could not be verified: {path}",
            new[]
            {
                new SkillStepResult("Create timestamped backup", true, path),
                new SkillStepResult("Replace text", verified, $"{count} occurrence(s)")
            });
    }

    private SkillResult ListFiles(string root)
    {
        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(path => !Path.GetFileName(path).Contains(".kb-backup-", StringComparison.OrdinalIgnoreCase))
            .Take(200)
            .Select(path => Path.GetRelativePath(root, path))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return new SkillResult(
            Id,
            SkillStatus.Completed,
            files.Length == 0 ? $"Website project is empty: {root}" : $"Website project files in {root}:\n{string.Join(Environment.NewLine, files.Select(path => $"• {path}"))}",
            new[] { new SkillStepResult("List project files", true, $"{files.Length} file(s)") });
    }

    private SkillResult ValidateProject(string root)
    {
        var html = Directory.EnumerateFiles(root, "*.html", SearchOption.AllDirectories).ToArray();
        var css = Directory.EnumerateFiles(root, "*.css", SearchOption.AllDirectories).ToArray();
        var js = Directory.EnumerateFiles(root, "*.js", SearchOption.AllDirectories).ToArray();
        var problems = new List<string>();
        foreach (var path in html)
        {
            var text = File.ReadAllText(path);
            if (!text.Contains("<html", StringComparison.OrdinalIgnoreCase)) problems.Add($"Missing <html> in {Path.GetRelativePath(root, path)}");
            if (!text.Contains("<title", StringComparison.OrdinalIgnoreCase)) problems.Add($"Missing <title> in {Path.GetRelativePath(root, path)}");
        }
        var valid = html.Length > 0 && problems.Count == 0;
        return new SkillResult(
            Id,
            valid ? SkillStatus.Completed : SkillStatus.Prepared,
            $"Project validation: {html.Length} HTML, {css.Length} CSS and {js.Length} JavaScript file(s)." + (problems.Count == 0 ? " No structural problems detected." : $"\n{string.Join(Environment.NewLine, problems)}"),
            new[] { new SkillStepResult("Validate website structure", valid, problems.Count == 0 ? "Basic structure passed." : string.Join(" | ", problems.Take(10))) });
    }

    private SkillResult OpenProject(string root)
    {
        OpenFolder(root);
        return new SkillResult(Id, SkillStatus.Completed, $"Opened website project: {root}", new[] { new SkillStepResult("Open project folder", true, root) });
    }

    private static async Task WriteWithBackupAsync(string path, string content, CancellationToken token)
    {
        if (File.Exists(path))
        {
            var backup = path + $".kb-backup-{DateTime.Now:yyyyMMdd-HHmmssfff}";
            File.Copy(path, backup, overwrite: false);
        }
        var temporary = path + ".kb-writing";
        await File.WriteAllTextAsync(temporary, content, new UTF8Encoding(false), token).ConfigureAwait(false);
        File.Move(temporary, path, overwrite: true);
    }

    private static string ResolveProjectRoot(string project)
    {
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var root = Path.IsPathRooted(project)
            ? Path.GetFullPath(Environment.ExpandEnvironmentVariables(project.Trim().Trim('"')))
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "KB Jarvis Websites", Sanitize(project));
        var fullProfile = Path.GetFullPath(profile).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullRoot = Path.GetFullPath(root);
        if (!fullRoot.StartsWith(fullProfile, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Website projects must be inside the current Windows user profile.");
        return fullRoot;
    }

    private static string ResolveProjectFile(string root, string relative)
    {
        var path = Path.GetFullPath(Path.Combine(root, relative.Trim().Trim('"').Replace('/', Path.DirectorySeparatorChar)));
        var rootPrefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("The requested file is outside the website project.");
        return path;
    }

    private static void OpenFolder(string path) => Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{path}\"", UseShellExecute = true });
    private static string RelativeAssetPrefix(string root, string pagePath) => string.Concat(Enumerable.Repeat("../", Math.Max(0, Path.GetRelativePath(root, Path.GetDirectoryName(pagePath)!).Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Length)));
    private static int CountOccurrences(string source, string value) { var count = 0; var index = 0; while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0) { count++; index += value.Length; } return count; }
    private static string Humanize(string value) => string.Join(' ', value.Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries).Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    private static string Sanitize(string value) { foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '-'); return string.IsNullOrWhiteSpace(value) ? "Website" : value.Trim(); }
    private static string Get(SkillRequest request, string key, string fallback) => request.Arguments.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    private static bool GetBool(SkillRequest request, string key, bool fallback) => request.Arguments.TryGetValue(key, out var value) && bool.TryParse(value, out var parsed) ? parsed : fallback;

    private static string DefaultHtml(string title) => $"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{System.Net.WebUtility.HtmlEncode(title)}</title>
  <link rel="stylesheet" href="css/style.css">
</head>
<body>
  <header class="site-header"><a class="brand" href="index.html">{System.Net.WebUtility.HtmlEncode(title)}</a></header>
  <main class="hero"><p class="eyebrow">KB JARVIS WEBSITE STUDIO</p><h1>{System.Net.WebUtility.HtmlEncode(title)}</h1><p>Professional custom website project ready for content and design.</p></main>
  <script src="js/app.js"></script>
</body>
</html>
""";

    private static string DefaultPageHtml(string title, string prefix) => $"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{System.Net.WebUtility.HtmlEncode(title)}</title>
  <link rel="stylesheet" href="{prefix}css/style.css">
</head>
<body>
  <main class="page"><h1>{System.Net.WebUtility.HtmlEncode(title)}</h1><p>Page content ready for editing.</p></main>
  <script src="{prefix}js/app.js"></script>
</body>
</html>
""";

    private static string DefaultCss() => """:root{color-scheme:dark;--bg:#05070b;--panel:#0c1620;--cyan:#2de0ff;--text:#eefcff;--muted:#8baab5}*{box-sizing:border-box}body{margin:0;background:radial-gradient(circle at 50% 20%,#102a3a,var(--bg) 60%);color:var(--text);font-family:Inter,Segoe UI,sans-serif;min-height:100vh}.site-header{padding:22px 6vw;border-bottom:1px solid #21485a;background:#050a0fcc}.brand{color:var(--cyan);font-weight:800;letter-spacing:.08em;text-decoration:none}.hero,.page{max-width:1100px;margin:auto;padding:12vh 6vw}.eyebrow{color:var(--cyan);font-size:.78rem;letter-spacing:.18em}.hero h1,.page h1{font-size:clamp(2.8rem,8vw,7rem);line-height:.94;margin:.25em 0}.hero p,.page p{color:var(--muted);font-size:1.15rem;max-width:700px;line-height:1.7}@media(max-width:700px){.hero,.page{padding-top:9vh}.hero h1,.page h1{font-size:3rem}}
""";

    private static string DefaultJavaScript() => """document.documentElement.classList.add('js-ready');
console.info('KB Jarvis website project loaded');
""";
}