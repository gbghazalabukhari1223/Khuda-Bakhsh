using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace KB.Jarvis.App.Services;

public sealed record JarvisSettings(
    string ApiKey,
    string LiveModel,
    string TextModel,
    string VoiceName,
    bool AutoReconnect,
    bool SpeakTextReplies)
{
    public static JarvisSettings Default { get; } = new(
        ApiKey: string.Empty,
        LiveModel: "gemini-3.1-flash-live-preview",
        TextModel: "gemini-3.6-flash",
        VoiceName: "Laomedeia",
        AutoReconnect: true,
        SpeakTextReplies: true);
}

internal sealed record StoredJarvisSettings(
    string EncryptedApiKey,
    string LiveModel,
    string TextModel,
    string VoiceName,
    bool AutoReconnect,
    bool SpeakTextReplies);

public sealed class JarvisSettingsStore
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("KB-Jarvis-OS-v12-Khuda-Bakhsh");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string SettingsDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KB Jarvis OS",
        "Config");

    public string SettingsPath => Path.Combine(SettingsDirectory, "settings.json");

    public JarvisSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
            {
                return JarvisSettings.Default;
            }

            var stored = JsonSerializer.Deserialize<StoredJarvisSettings>(File.ReadAllText(SettingsPath));
            if (stored is null)
            {
                return JarvisSettings.Default;
            }

            return new JarvisSettings(
                Decrypt(stored.EncryptedApiKey),
                string.IsNullOrWhiteSpace(stored.LiveModel) ? JarvisSettings.Default.LiveModel : stored.LiveModel,
                string.IsNullOrWhiteSpace(stored.TextModel) ? JarvisSettings.Default.TextModel : stored.TextModel,
                string.IsNullOrWhiteSpace(stored.VoiceName) ? JarvisSettings.Default.VoiceName : stored.VoiceName,
                stored.AutoReconnect,
                stored.SpeakTextReplies);
        }
        catch (Exception exception)
        {
            Core.JarvisLog.Error("Could not load Jarvis settings", exception);
            return JarvisSettings.Default;
        }
    }

    public void Save(JarvisSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        Directory.CreateDirectory(SettingsDirectory);
        var stored = new StoredJarvisSettings(
            Encrypt(settings.ApiKey.Trim()),
            settings.LiveModel.Trim(),
            settings.TextModel.Trim(),
            settings.VoiceName.Trim(),
            settings.AutoReconnect,
            settings.SpeakTextReplies);
        var temporary = SettingsPath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(stored, JsonOptions), Encoding.UTF8);
        File.Move(temporary, SettingsPath, overwrite: true);
    }

    private static string Encrypt(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var protectedBytes = ProtectedData.Protect(
            Encoding.UTF8.GetBytes(value),
            Entropy,
            DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(protectedBytes);
    }

    private static string Decrypt(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        try
        {
            var unprotected = ProtectedData.Unprotect(
                Convert.FromBase64String(value),
                Entropy,
                DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(unprotected);
        }
        catch
        {
            return string.Empty;
        }
    }
}
