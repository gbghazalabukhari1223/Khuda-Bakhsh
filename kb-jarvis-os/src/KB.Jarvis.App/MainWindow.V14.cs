using System.Windows;
using KB.Jarvis.App.Skills;

namespace KB.Jarvis.App;

public partial class MainWindow
{
    public void InitializeV14()
    {
        _skills.Register(new FileOrganizerSkill());
        _skills.Register(new WebsiteProjectSkill());
        _skills.Register(new YouTubeSkill(_browserBridge));
        _skills.Register(new WordPressContentSkill(_browserBridge));
        InitializeV13();
        SkillCountText.Text = $"{_skills.Skills.Count} TRAINED";
        AddLog("Jarvis 14 performance queues, File Organizer, Website Studio, YouTube playback and WordPress content operator loaded.");
        SetMissionState(
            "Jarvis 14 Work Core ready",
            "Voice is queue-stabilized. File organization, custom website editing, YouTube playback and existing-session WordPress work are available.",
            "READY",
            System.Windows.Media.Brushes.LightGreen);
    }

    private async void OrganizeNotes_Click(object sender, RoutedEventArgs e) =>
        await ExecutePresetCommandAsync("Organize all Notepad text files on my Desktop into a folder named Organized Notepad Files");

    private void YouTubeTemplate_Click(object sender, RoutedEventArgs e)
    {
        CommandBox.Text = "Open YouTube and play ";
        CommandBox.Focus();
        CommandBox.CaretIndex = CommandBox.Text.Length;
    }

    private async void WebsiteStudio_Click(object sender, RoutedEventArgs e) =>
        await ExecutePresetCommandAsync("Create a new custom HTML CSS JavaScript website project named Boss Website and open it");

    private void WordPressTemplate_Click(object sender, RoutedEventArgs e)
    {
        CommandBox.Text = "Create a WordPress draft post titled  with content ";
        CommandBox.Focus();
        CommandBox.CaretIndex = CommandBox.Text.Length;
    }
}