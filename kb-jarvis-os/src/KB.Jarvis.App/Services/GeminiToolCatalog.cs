namespace KB.Jarvis.App.Services;

public static class GeminiToolCatalog
{
    public const string SystemInstruction = """
You are KB Jarvis, a mature multilingual Windows operating and work assistant created and developed by KB (Khuda Bakhsh). Address the user as Boss. Understand English, Urdu, Roman Urdu and Hindi, including informal, incomplete and mixed-language instructions. Translate the Boss's intent into an end-to-end plan and use local tools rather than merely explaining what the Boss can do.

Operational rules:
1. Never claim a computer action succeeded unless the local tool result says it completed or was verified.
2. For WhatsApp messaging, NEVER call open_application, never create a new WhatsApp tab, never refresh WhatsApp, and never navigate away from the existing signed-in session. Always call whatsapp_message. If no existing signed-in tab is available, report the exact blocker.
3. When screen or camera vision is enabled, use the latest visual frames as current context. For onscreen work, take one desktop_input action at a time, observe the next frame, then continue. Do not guess blindly when visual evidence is available.
4. Use search_files to locate files and folders. Use organize_files to create folders and move or copy matching files. Moving files requires the application's one-confirmation workflow.
5. Use website_project for custom HTML, CSS, JavaScript and PHP website projects. Create automatic backups before changing existing files, validate results, and keep projects inside the current Windows user profile.
6. Use youtube_play to open YouTube, search for the requested song or video, select a normal video result and verify playback.
7. Use wordpress_content only with an existing signed-in wp-admin tab. Save drafts without repeated confirmation. Publishing requires one final confirmation. Content may come from your own writing or the latest readable response in an existing ChatGPT tab.
8. Use windows_search for the Windows taskbar or Start search box.
9. Consequential actions such as sending messages, publishing, deleting, purchasing, moving files and Windows power actions require the application's confirmation workflow.
10. Ask one concise clarification only when an essential target, message, path, file or authentication detail is genuinely missing.
11. If asked who created you, say: Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal multimodal Windows operating and website work assistant hoon.
""";

    public static object[] CreateFunctionDeclarations() =>
    [
        new
        {
            name = "execute_local_goal",
            description = "Execute a trained Windows, file, website or browser goal locally and return verified results.",
            parameters = new
            {
                type = "object",
                properties = new { goal = new { type = "string", description = "Complete user goal in natural language." } },
                required = new[] { "goal" }
            }
        },
        new
        {
            name = "whatsapp_message",
            description = "Inspect, draft or send in the user's EXISTING signed-in WhatsApp Web tab. Never opens a new WhatsApp tab. Optionally finds a contact first.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", @enum = new[] { "inspect", "draft", "send" } },
                    contact = new { type = "string", description = "Optional contact or chat name. Omit to use the currently open chat." },
                    message = new { type = "string", description = "Message text. Required for draft or send." }
                },
                required = new[] { "action" }
            }
        },
        new
        {
            name = "open_application",
            description = "Open a supported Windows application such as Notepad, Calculator, Paint, File Explorer, Task Manager, Settings, Chrome, Edge or VS Code. Do not use for WhatsApp messaging or YouTube playback.",
            parameters = new
            {
                type = "object",
                properties = new { application = new { type = "string" } },
                required = new[] { "application" }
            }
        },
        new
        {
            name = "write_notepad_note",
            description = "Create a text note, verify it on disk and open it in Notepad.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    content = new { type = "string" },
                    path = new { type = "string", description = "Optional complete Windows path." }
                },
                required = new[] { "content" }
            }
        },
        new
        {
            name = "list_browser_tabs",
            description = "List the user's existing Chrome tabs through the Browser Companion.",
            parameters = new { type = "object", properties = new { } }
        },
        new
        {
            name = "search_files",
            description = "Search standard local user folders for files or folders and optionally open the best match.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string" },
                    kind = new { type = "string", @enum = new[] { "any", "file", "folder" } },
                    open = new { type = "boolean" }
                },
                required = new[] { "query" }
            }
        },
        new
        {
            name = "organize_files",
            description = "Create a destination folder and organize matching local files by moving or copying them. Examples include moving all Desktop .txt Notepad files into one folder.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", @enum = new[] { "organize", "create_folder" } },
                    source = new { type = "string", description = "Source folder path or Desktop, Documents or Downloads." },
                    destination = new { type = "string", description = "Complete destination folder path. May be omitted when folder_name is supplied." },
                    folder_name = new { type = "string", description = "New folder name inside the source folder." },
                    pattern = new { type = "string", description = "File pattern such as *.txt, *.html or *.pdf." },
                    mode = new { type = "string", @enum = new[] { "move", "copy" } },
                    recursive = new { type = "boolean" },
                    open = new { type = "boolean" }
                },
                required = new[] { "action" }
            }
        },
        new
        {
            name = "website_project",
            description = "Create, list, validate and safely edit a custom HTML, CSS, JavaScript or PHP website project. Existing files receive timestamped backups.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", @enum = new[] { "create_project", "create_page", "write_file", "update_file", "search_replace", "list_files", "validate", "open_project" } },
                    project = new { type = "string", description = "Project name or a complete path inside the current Windows user profile." },
                    path = new { type = "string", description = "Relative project file path such as index.html, css/style.css or pages/about.html." },
                    slug = new { type = "string" },
                    title = new { type = "string" },
                    content = new { type = "string" },
                    search = new { type = "string" },
                    replace = new { type = "string" },
                    open = new { type = "boolean" }
                },
                required = new[] { "action", "project" }
            }
        },
        new
        {
            name = "youtube_play",
            description = "Open or reuse YouTube, search for a requested song or video, open the first normal video result and verify playback.",
            parameters = new
            {
                type = "object",
                properties = new { query = new { type = "string" } },
                required = new[] { "query" }
            }
        },
        new
        {
            name = "wordpress_content",
            description = "Create or update a WordPress page or post in an EXISTING signed-in wp-admin tab. Save draft by default; publishing is confirmation protected. source=chatgpt reads the latest assistant response from an existing ChatGPT tab when content is omitted.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", @enum = new[] { "create_post", "create_page", "update_current" } },
                    title = new { type = "string" },
                    content = new { type = "string" },
                    source = new { type = "string", @enum = new[] { "self", "chatgpt" } },
                    status = new { type = "string", @enum = new[] { "draft", "publish" } }
                },
                required = new[] { "action", "status" }
            }
        },
        new
        {
            name = "windows_search",
            description = "Open the Windows Start/taskbar search, enter a query and optionally open the top result.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    query = new { type = "string" },
                    open = new { type = "boolean" }
                },
                required = new[] { "query" }
            }
        },
        new
        {
            name = "desktop_input",
            description = "Perform one authorized native Windows mouse or keyboard action using the current visual frame. Coordinates are normalized from 0 to 1000 across the full virtual desktop.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", @enum = new[] { "click", "double_click", "right_click", "type", "keypress", "scroll" } },
                    x = new { type = "integer", minimum = 0, maximum = 1000 },
                    y = new { type = "integer", minimum = 0, maximum = 1000 },
                    text = new { type = "string" },
                    key = new { type = "string", description = "ENTER, TAB, ESCAPE, BACKSPACE, DELETE, UP, DOWN, LEFT, RIGHT, CTRL_A, CTRL_C, CTRL_V, CTRL_S, ALT_TAB or WIN." },
                    amount = new { type = "integer", description = "Scroll notches; positive scrolls up and negative scrolls down." }
                },
                required = new[] { "action" }
            }
        }
    ];
}