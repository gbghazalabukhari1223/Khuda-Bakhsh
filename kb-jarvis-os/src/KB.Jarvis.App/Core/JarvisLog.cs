using System.IO;
using System.Text;

namespace KB.Jarvis.App.Core;

public static class JarvisLog
{
    private static readonly object Gate = new();
    private static readonly string LogDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KB Jarvis OS",
        "Logs");

    public static string CurrentLogPath { get; } = Path.Combine(
        LogDirectory,
        $"jarvis-{DateTime.Now:yyyy-MM-dd}.log");

    public static void Info(string message) => Write("INFO", message, null);
    public static void Warning(string message) => Write("WARN", message, null);
    public static void Error(string message, Exception? exception = null) => Write("ERROR", message, exception);

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
            var builder = new StringBuilder()
                .Append('[').Append(DateTimeOffset.Now.ToString("O")).Append("] ")
                .Append(level).Append(" | ")
                .Append(message);

            if (exception is not null)
            {
                builder.AppendLine().Append(exception);
            }

            lock (Gate)
            {
                File.AppendAllText(CurrentLogPath, builder.AppendLine().ToString(), Encoding.UTF8);
            }
        }
        catch
        {
            // Logging must never crash the assistant.
        }
    }
}
