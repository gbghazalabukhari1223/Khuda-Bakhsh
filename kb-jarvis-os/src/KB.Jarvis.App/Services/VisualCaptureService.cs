using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using KB.Jarvis.App.Native;
using OpenCvSharp;

namespace KB.Jarvis.App.Services;

public sealed class VisualCaptureService : IAsyncDisposable
{
    private CancellationTokenSource? _screenCts;
    private CancellationTokenSource? _cameraCts;
    private Task? _screenTask;
    private Task? _cameraTask;

    public event Action<string, byte[]>? FrameReady;
    public event Action<string>? Diagnostic;

    public bool ScreenRunning => _screenTask is { IsCompleted: false };
    public bool CameraRunning => _cameraTask is { IsCompleted: false };

    public Task StartScreenAsync(CancellationToken lifetimeToken)
    {
        if (ScreenRunning) return Task.CompletedTask;
        _screenCts = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);
        VisualContextHub.ScreenEnabled = true;
        _screenTask = Task.Run(() => ScreenLoopAsync(_screenCts.Token), _screenCts.Token);
        Diagnostic?.Invoke("Live screen vision started.");
        return Task.CompletedTask;
    }

    public async Task StopScreenAsync()
    {
        VisualContextHub.ScreenEnabled = false;
        var cts = _screenCts;
        _screenCts = null;
        try { cts?.Cancel(); } catch { }
        if (_screenTask is not null)
        {
            try { await _screenTask.ConfigureAwait(false); } catch (OperationCanceledException) { }
            _screenTask = null;
        }
        cts?.Dispose();
        Diagnostic?.Invoke("Live screen vision stopped.");
    }

    public Task StartCameraAsync(CancellationToken lifetimeToken)
    {
        if (CameraRunning) return Task.CompletedTask;
        _cameraCts = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);
        VisualContextHub.CameraEnabled = true;
        _cameraTask = Task.Run(() => CameraLoopAsync(_cameraCts.Token), _cameraCts.Token);
        Diagnostic?.Invoke("Live camera vision started.");
        return Task.CompletedTask;
    }

    public async Task StopCameraAsync()
    {
        VisualContextHub.CameraEnabled = false;
        var cts = _cameraCts;
        _cameraCts = null;
        try { cts?.Cancel(); } catch { }
        if (_cameraTask is not null)
        {
            try { await _cameraTask.ConfigureAwait(false); } catch (OperationCanceledException) { }
            _cameraTask = null;
        }
        cts?.Dispose();
        Diagnostic?.Invoke("Live camera vision stopped.");
    }

    private async Task ScreenLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var jpeg = CaptureScreenJpeg();
                VisualContextHub.Update("screen", jpeg);
                FrameReady?.Invoke("screen", jpeg);
            }
            catch (Exception exception)
            {
                Diagnostic?.Invoke($"Screen capture failed: {exception.Message}");
            }
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task CameraLoopAsync(CancellationToken cancellationToken)
    {
        using var capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
        capture.Set(VideoCaptureProperties.FrameWidth, 1280);
        capture.Set(VideoCaptureProperties.FrameHeight, 720);
        if (!capture.IsOpened())
        {
            VisualContextHub.CameraEnabled = false;
            Diagnostic?.Invoke("No accessible camera device was found.");
            return;
        }

        using var frame = new Mat();
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (capture.Read(frame) && !frame.Empty())
                {
                    Cv2.ImEncode(
                        ".jpg",
                        frame,
                        out var jpeg,
                        new ImageEncodingParam(ImwriteFlags.JpegQuality, 72));
                    VisualContextHub.Update("camera", jpeg);
                    FrameReady?.Invoke("camera", jpeg);
                }
            }
            catch (Exception exception)
            {
                Diagnostic?.Invoke($"Camera capture failed: {exception.Message}");
            }
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);
        }
    }

    private static byte[] CaptureScreenJpeg()
    {
        var bounds = NativeInput.GetVirtualScreenGeometry();
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            throw new InvalidOperationException("Windows returned invalid virtual-screen dimensions.");
        }

        using var full = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(full))
        {
            graphics.CopyFromScreen(
                bounds.Left,
                bounds.Top,
                0,
                0,
                new System.Drawing.Size(bounds.Width, bounds.Height),
                CopyPixelOperation.SourceCopy);
        }

        const int maximumWidth = 1280;
        var targetWidth = Math.Min(maximumWidth, full.Width);
        var targetHeight = Math.Max(1, (int)Math.Round(full.Height * targetWidth / (double)full.Width));
        using var resized = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(resized))
        {
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(full, 0, 0, targetWidth, targetHeight);
        }

        using var stream = new MemoryStream();
        var codec = ImageCodecInfo.GetImageEncoders().First(encoder => encoder.FormatID == ImageFormat.Jpeg.Guid);
        using var parameters = new EncoderParameters(1);
        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 70L);
        resized.Save(stream, codec, parameters);
        return stream.ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        await StopScreenAsync().ConfigureAwait(false);
        await StopCameraAsync().ConfigureAwait(false);
    }
}