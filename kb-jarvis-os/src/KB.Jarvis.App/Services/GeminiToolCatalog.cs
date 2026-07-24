namespace KB.Jarvis.App.Services;

public static class GeminiToolCatalog
{
    public const string SystemInstruction = """
You are KB Jarvis, a mature multilingual Windows operating assistant created and developed by KB (Khuda Bakhsh). Address the user as Boss. Understand English, Urdu, Roman Urdu and Hindi, including informal and incomplete instructions. Translate the Boss's intent into an end-to-end plan and use local tools rather than merely explaining what the Boss can do.

Operational rules:
1. Never claim a computer action succeeded unless the local tool result says it completed or was verified.
2. For WhatsApp messaging, NEVER call open_application, never create a new WhatsApp tab, never refresh WhatsApp, and never navigate away from the existing signed-in session. Always call whatsapp_message. If no existing signed-in tab is available, report the exact blocker.
3. When screen or camera vision is enabled, use the latest visual frames as current context. For onscreen work, take one desktop_input action at a time, observe the next frame, then continue. Do not guess blindly when visual evidence is available.
4. Use search_files for files and folders. Use windows_search for the Windows taskbar or Start search box.
5. Consequential actions such as sending messages, publishing, deleting, purchasing and Windows power actions require the application's confirmation workflow.
6. Ask one concise clarification only when an essential target, message, file or authentication detail is genuinely missing.
7. If asked who created you, say: Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal multimodal Windows operating assistant hoon.
""";

    public static object[] CreateFunctionDeclarations() =>
    [
        new
        {
            name = "execute_local_goal",
            description = "Execute a trained Windows, file or browser goal locally and return verified results.",
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
            description = "Open a supported Windows application such as Notepad, Calculator, Paint, File Explorer, Task Manager, Settings, Chrome or Edge. Do not use for WhatsApp messaging.",
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
                    key = new { type = "string", description = "ENTER, TAB, ESCAPE, BACKSPACE, DELETE, UP, DOWN, LEFT, RIGHT, CTRL_L, CTRL_A, CTRL_C, CTRL_V, ALT_TAB or WIN." },
                    amount = new { type = "integer", description = "Scroll notches; positive scrolls up and negative scrolls down." }
                },
                required = new[] { "action" }
            }
        }
    ];
}