using System.Text.Json.Nodes;

namespace KB.Jarvis.App.Services;

public sealed record VisualFrame(string Source, byte[] Jpeg, DateTimeOffset CapturedAt);

public static class VisualContextHub
{
    private static readonly object Gate = new();
    private static VisualFrame? _screen;
    private static VisualFrame? _camera;

    public static bool ScreenEnabled { get; set; }
    public static bool CameraEnabled { get; set; }

    public static void Update(string source, byte[] jpeg)
    {
        ArgumentNullException.ThrowIfNull(jpeg);
        var frame = new VisualFrame(source, jpeg.ToArray(), DateTimeOffset.UtcNow);
        lock (Gate)
        {
            if (string.Equals(source, "screen", StringComparison.OrdinalIgnoreCase))
            {
                _screen = frame;
            }
            else if (string.Equals(source, "camera", StringComparison.OrdinalIgnoreCase))
            {
                _camera = frame;
            }
        }
    }

    public static IReadOnlyList<VisualFrame> Snapshot()
    {
        lock (Gate)
        {
            var frames = new List<VisualFrame>(2);
            if (ScreenEnabled && _screen is not null)
            {
                frames.Add(_screen with { Jpeg = _screen.Jpeg.ToArray() });
            }
            if (CameraEnabled && _camera is not null)
            {
                frames.Add(_camera with { Jpeg = _camera.Jpeg.ToArray() });
            }
            return frames;
        }
    }

    public static JsonArray CreateInlineParts(string contextLabel)
    {
        var parts = new JsonArray();
        foreach (var frame in Snapshot())
        {
            parts.Add(new JsonObject
            {
                ["text"] = $"{contextLabel}: latest {frame.Source} frame captured at {frame.CapturedAt:O}."
            });
            parts.Add(new JsonObject
            {
                ["inlineData"] = new JsonObject
                {
                    ["mimeType"] = "image/jpeg",
                    ["data"] = Convert.ToBase64String(frame.Jpeg)
                }
            });
        }
        return parts;
    }
}