namespace KB.Jarvis.App.Core;

public static class Identity
{
    public const string ProductName = "KB Jarvis OS";
    public const string AssistantName = "Jarvis";
    public const string DeveloperShort = "KB";
    public const string DeveloperFull = "KB (Khuda Bakhsh)";
    public const string Version = "13.0.0";
    public const string PrimaryUserTitle = "Boss";

    public static string CreatorResponse(string languageHint = "roman-urdu") => languageHint.ToLowerInvariant() switch
    {
        "english" or "en" => "I was designed and developed by KB — Khuda Bakhsh. I am his personal multimodal Windows operating assistant.",
        "urdu" or "ur" => "مجھے کے بی — خدا بخش نے ڈیزائن اور ڈیولپ کیا ہے۔ میں ان کا ذاتی ملٹی موڈل ونڈوز آپریٹنگ اسسٹنٹ ہوں۔",
        _ => "Mujhe KB — Khuda Bakhsh ne design aur develop kiya hai. Main unka personal multimodal Windows operating assistant hoon."
    };
}