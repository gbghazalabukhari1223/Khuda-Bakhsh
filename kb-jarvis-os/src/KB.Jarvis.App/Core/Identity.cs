namespace KB.Jarvis.App.Core;

public static class Identity
{
    public const string ProductName = "KB Jarvis OS";
    public const string AssistantName = "Jarvis";
    public const string DeveloperShort = "KB";
    public const string DeveloperFull = "KB (Khuda Bakhsh)";
    public const string Version = "15.0.0";
    public const string PrimaryUserTitle = "Boss";

    public static string CreatorResponse(string languageHint = "roman-urdu") => languageHint.ToLowerInvariant() switch
    {
        "english" or "en" => "I was designed and developed by KB — Khuda Bakhsh. I am his professional female multimodal Windows, browser and website work assistant.",
        "urdu" or "ur" => "مجھے کے بی — خدا بخش نے ڈیزائن اور ڈیولپ کیا ہے۔ میں ان کی پروفیشنل خاتون ملٹی موڈل ونڈوز، براؤزر اور ویب سائٹ ورک اسسٹنٹ ہوں۔",
        _ => "Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unki professional female multimodal Windows, browser aur website work assistant hoon."
    };
}
