using System.Windows;
using KB.Jarvis.App.Services;

namespace KB.Jarvis.App;

public partial class SettingsWindow : Window
{
    public JarvisSettings Settings { get; private set; }

    public SettingsWindow(JarvisSettings settings)
    {
        InitializeComponent();
        Settings = settings;
        ApiKeyBox.Password = settings.ApiKey;
        LiveModelBox.Text = settings.LiveModel;
        TextModelBox.Text = settings.TextModel;
        VoiceNameBox.Text = settings.VoiceName;
        AutoReconnectBox.IsChecked = settings.AutoReconnect;
        SpeakRepliesBox.IsChecked = settings.SpeakTextReplies;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ApiKeyBox.Password))
        {
            MessageBox.Show(this, "Please enter a Gemini API key.", "KB Jarvis Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Settings = new JarvisSettings(
            ApiKeyBox.Password.Trim(),
            string.IsNullOrWhiteSpace(LiveModelBox.Text) ? JarvisSettings.Default.LiveModel : LiveModelBox.Text.Trim(),
            string.IsNullOrWhiteSpace(TextModelBox.Text) ? JarvisSettings.Default.TextModel : TextModelBox.Text.Trim(),
            string.IsNullOrWhiteSpace(VoiceNameBox.Text) ? JarvisSettings.Default.VoiceName : VoiceNameBox.Text.Trim(),
            AutoReconnectBox.IsChecked == true,
            SpeakRepliesBox.IsChecked == true);
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}