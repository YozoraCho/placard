using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Placard.Core.Localization;

[JsonSerializable(typeof(Dictionary<string, string>))]
internal sealed partial class LocalizationJsonContext : JsonSerializerContext
{
}

internal static class Loc
{
    private static readonly Dictionary<string, string> UpperCache = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, CompositeFormat> FormatCache = new(StringComparer.Ordinal);

    private static string directory = string.Empty;
    private static LanguageInfo current = Languages.English;
    private static CultureInfo culture = CultureInfo.InvariantCulture;
    private static StringCatalog catalog = StringCatalog.Empty;

    public static LanguageInfo Current => current;

    public static CultureInfo Culture => culture;

    public static void Initialize(string code, string localizationDirectory)
    {
        directory = localizationDirectory;
        Apply(Languages.Resolve(code));
    }

    public static void SetLanguage(string code)
    {
        var target = Languages.Resolve(code);
        if (ReferenceEquals(target, current))
        {
            return;
        }

        Apply(target);
    }

    public static string Upper(string text)
    {
        if (UpperCache.TryGetValue(text, out var cached))
        {
            return cached;
        }

        var upper = culture.TextInfo.ToUpper(text);
        UpperCache[text] = upper;
        return upper;
    }

    public static string T(LocString entry) => catalog.TryGet(entry.Key, out var value) ? value : entry.Source;

    public static string T<T0>(LocString entry, T0 arg0) => string.Format(culture, FormatFor(T(entry)), arg0);

    public static string T<T0, T1>(LocString entry, T0 arg0, T1 arg1) =>
        string.Format(culture, FormatFor(T(entry)), arg0, arg1);

    public static string T<T0, T1, T2>(LocString entry, T0 arg0, T1 arg1, T2 arg2) =>
        string.Format(culture, FormatFor(T(entry)), arg0, arg1, arg2);

    public static string T(LocString entry, params object[] args) => string.Format(culture, T(entry), args);

    public static string Plural(LocPlural entry, int count)
    {
        var template = IsOne(count)
            ? Resolve(string.Concat(entry.KeyBase, ".one"), entry.OneSource)
            : Resolve(string.Concat(entry.KeyBase, ".other"), entry.OtherSource);
        return string.Format(culture, FormatFor(template), count);
    }

    private static CompositeFormat FormatFor(string template)
    {
        if (FormatCache.TryGetValue(template, out var cached))
        {
            return cached;
        }

        var parsed = CompositeFormat.Parse(template);
        FormatCache[template] = parsed;
        return parsed;
    }

    private static string Resolve(string key, string source) => catalog.TryGet(key, out var value) ? value : source;

    private static bool IsOne(int count)
    {
        var magnitude = Math.Abs(count);
        return current.PluralKind switch
        {
            PluralKind.French => magnitude is 0 or 1,
            _ => magnitude == 1,
        };
    }

    private static void Apply(LanguageInfo language)
    {
        current = language;
        culture = ResolveCulture(language.CultureName);
        UpperCache.Clear();
        FormatCache.Clear();
        catalog = ReferenceEquals(language, Languages.English)
            ? StringCatalog.Empty
            : StringCatalog.Load(Path.Combine(directory, string.Concat(language.Code, ".json")));
    }

    private static CultureInfo ResolveCulture(string name)
    {
        try
        {
            return CultureInfo.GetCultureInfo(name);
        }
        catch (CultureNotFoundException exception)
        {
            PlacardLog.Warning(exception, $"Culture '{name}' is unavailable; falling back to the invariant culture");
            return CultureInfo.InvariantCulture;
        }
    }
}
