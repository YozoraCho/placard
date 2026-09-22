namespace Placard.Core.Localization;

internal enum PluralKind
{
    EnglishLike,
    French,
}

internal sealed record LanguageInfo(string Code, string NativeName, string EnglishName, string CultureName,
    PluralKind PluralKind);

internal static class Languages
{
    public static readonly LanguageInfo English = new("en", "English", "English", "en-US", PluralKind.EnglishLike);
    public static readonly LanguageInfo French = new("fr", "Français", "French", "fr-FR", PluralKind.French);
    public static readonly LanguageInfo German = new("de", "Deutsch", "German", "de-DE", PluralKind.EnglishLike);
    public static readonly LanguageInfo Spanish = new("es", "Español", "Spanish", "es-ES", PluralKind.EnglishLike);

    public static readonly LanguageInfo Portuguese =
        new("pt", "Português", "Portuguese", "pt-BR", PluralKind.EnglishLike);

    public static readonly LanguageInfo Turkish = new("tr", "Türkçe", "Turkish", "tr-TR",
        PluralKind.EnglishLike);

    public static readonly LanguageInfo Russian = new("ru", "Русский", "Russian",
        "ru-RU", PluralKind.EnglishLike);

    public static readonly LanguageInfo Japanese = new("ja", "日本語", "Japanese", "ja-JP",
        PluralKind.EnglishLike);

    public static readonly LanguageInfo Chinese = new("zh", "中文", "Chinese", "zh-CN",
        PluralKind.EnglishLike);

    public static readonly LanguageInfo[] All =
    [
        English, French, German, Spanish, Portuguese, Turkish, Russian, Japanese, Chinese,
    ];

    public static LanguageInfo Resolve(string? code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return English;
        }

        for (var index = 0; index < All.Length; index++)
        {
            if (string.Equals(All[index].Code, code, StringComparison.OrdinalIgnoreCase))
            {
                return All[index];
            }
        }

        return English;
    }
}
