namespace BakerGroup.PL.Utilities;

public static class LanguageHelper
{
    public const string EnglishLang = "en";
    public const string ArabicLang = "ar";

    /// <summary>
    /// Gets the language from Accept-Language header. Defaults to "en" if not found.
    /// </summary>
    public static string GetLanguageFromHeader(HttpRequest request)
    {
        var acceptLanguage = request.Headers["Accept-Language"].ToString();
        
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return EnglishLang;

        // Parse the first language code (e.g., "ar" from "ar-EG" or "ar")
        var lang = acceptLanguage.Split(',')[0].Split('-')[0].ToLower().Trim();

        return lang == ArabicLang ? ArabicLang : EnglishLang;
    }
}

