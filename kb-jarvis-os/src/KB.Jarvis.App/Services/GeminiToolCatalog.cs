namespace KB.Jarvis.App.Services;

public static class GeminiToolCatalog
{
    public const string SystemInstruction = """
You are KB Jarvis, a mature multilingual Windows operating assistant created and developed by KB (Khuda Bakhsh). Address the user as Boss. Understand English, Urdu, Roman Urdu and Hindi. Never claim a computer action succeeded unless the local tool result says it was completed and verified. Do not tell Boss to perform a supported task manually; call the appropriate tool. Ask one concise clarification only when essential information is missing. Consequential actions such as sending messages require the application's confirmation workflow. If asked who created you, say: Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal AI operating assistant hoon.
""";

    public static object[] CreateFunctionDeclarations() =>
    [
        new
        {
            name = "execute_local_goal",
            description = "Execute a Windows, file, browser or trained Jarvis goal locally and return verified results.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    goal = new { type = "string", description = "Complete user goal in natural language." }
                },
                required = new[] { "goal" }
            }
        },
        new
        {
            name = "whatsapp_current_chat",
            description = "Inspect, draft in, or send to the currently open WhatsApp Web chat using the existing signed-in browser tab.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    action = new { type = "string", @enum = new[] { "inspect", "draft", "send" } },
                    message = new { type = "string", description = "Message text. Required for draft or send." }
                },
                required = new[] { "action" }
            }
        },
        new
        {
            name = "open_application",
            description = "Open a supported Windows application such as Notepad, Calculator, Paint, File Explorer, Task Manager, Settings, Chrome or Edge.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    application = new { type = "string" }
                },
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
            parameters = new
            {
                type = "object",
                properties = new { }
            }
        }
    ];
}