using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KB.Jarvis.App.Services;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow
{
    private readonly VisualCaptureService _visualV13 = new();
    private long _lastLiveVisualTicksV13;

    public void InitializeV13()
    {
        _skills.Register(new FileSearchSkill());
        _skills.Register(new WindowsSearchSkill());
        InitializeV12();

        _visualV13.FrameReady += VisualFrameReadyV13;
        _visualV13.Diagnostic += message => Dispatcher.BeginInvoke(() => AddLog($"VISION · {message}"));
        SizeChanged += MainWindow_SizeChanged;
        Closing += (_, _) => _ = _visualV13.DisposeAsync().AsTask();
        ApplyResponsiveLayoutV13(ActualWidth > 0 ? ActualWidth : Width);
        UpdateVisualControlsV13();
        AddLog("Jarvis 13 visual context, responsive layout, PC file search and Windows taskbar search loaded.");
    }

    private void VisualFrameReadyV13(string source, byte[] jpeg)
    {
        Dispatcher.BeginInvoke(() =>
        {
            var image = ToImageSourceV13(jpeg);
            if (string.Equals(source, "screen", StringComparison.OrdinalIgnoreCase))
            {
                ScreenPreviewImage.Source = image;
                ScreenVisionStatusText.Text = "LIVE";
                ScreenVisionStatusText.Foreground = Brushes.LightGreen;
            }
            else
            {
                CameraPreviewImage.Source = image;
                CameraVisionStatusText.Text = "LIVE";
                CameraVisionStatusText.Foreground = Brushes.LightGreen;
            }
        });

        if (!_liveVoiceV12.IsRunning) return;
        var now = Environment.TickCount64;
        var previous = Interlocked.Read(ref _lastLiveVisualTicksV13);
        if (now - previous < 1000) return;
        if (Interlocked.CompareExchange(ref _lastLiveVisualTicksV13, now, previous) != previous) return;
        _ = _liveVoiceV12.SendVideoFrameAsync(jpeg, source, _lifetime.Token);
    }

    private static BitmapImage ToImageSourceV13(byte[] jpeg)
    {
        using var stream = new MemoryStream(jpeg, writable: false);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }

    private async void ToggleScreenVision_Click(object sender, RoutedEventArgs e)
    {
        if (_visualV13.ScreenRunning)
        {
            await _visualV13.StopScreenAsync();
        }
        else
        {
            await _visualV13.StartScreenAsync(_lifetime.Token);
        }
        UpdateVisualControlsV13();
    }

    private async void ToggleCameraVision_Click(object sender, RoutedEventArgs e)
    {
        if (_visualV13.CameraRunning)
        {
            await _visualV13.StopCameraAsync();
        }
        else
        {
            await _visualV13.StartCameraAsync(_lifetime.Token);
        }
        UpdateVisualControlsV13();
    }

    private void UpdateVisualControlsV13()
    {
        ScreenVisionButton.Content = _visualV13.ScreenRunning ? "STOP SCREEN" : "START SCREEN";
        CameraVisionButton.Content = _visualV13.CameraRunning ? "STOP CAMERA" : "START CAMERA";
        if (!_visualV13.ScreenRunning)
        {
            ScreenVisionStatusText.Text = "OFF";
            ScreenVisionStatusText.Foreground = Brushes.Gold;
        }
        if (!_visualV13.CameraRunning)
        {
            CameraVisionStatusText.Text = "OFF";
            CameraVisionStatusText.Foreground = Brushes.Gold;
        }
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) => ApplyResponsiveLayoutV13(e.NewSize.Width);

    private void ApplyResponsiveLayoutV13(double width)
    {
        if (width >= 1300)
        {
            LeftColumn.Width = new GridLength(220);
            RightColumn.Width = new GridLength(330);
            LeftPanel.Visibility = Visibility.Visible;
            RightPanel.Visibility = Visibility.Visible;
        }
        else if (width >= 1000)
        {
            LeftColumn.Width = new GridLength(180);
            RightColumn.Width = new GridLength(265);
            LeftPanel.Visibility = Visibility.Visible;
            RightPanel.Visibility = Visibility.Visible;
        }
        else if (width >= 820)
        {
            LeftColumn.Width = new GridLength(160);
            RightColumn.Width = new GridLength(0);
            LeftPanel.Visibility = Visibility.Visible;
            RightPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            LeftColumn.Width = new GridLength(0);
            RightColumn.Width = new GridLength(0);
            LeftPanel.Visibility = Visibility.Collapsed;
            RightPanel.Visibility = Visibility.Collapsed;
        }
    }
}