using System.Text.Json;

namespace Placard.Core.Localization;

internal sealed class StringCatalog
{
    public static readonly StringCatalog Empty = new(new Dictionary<string, string>(0, StringComparer.Ordinal));

    private readonly Dictionary<string, string> entries;

    private StringCatalog(Dictionary<string, string> entries)
    {
        this.entries = entries;
    }

    public int Count => entries.Count;

    public bool TryGet(string key, out string value) => entries.TryGetValue(key, out value!);

    public static StringCatalog Load(string path)
    {
        if (!File.Exists(path))
        {
            return Empty;
        }

        try
        {
            using var stream = File.OpenRead(path);
            var parsed = JsonSerializer.Deserialize(stream, LocalizationJsonContext.Default.DictionaryStringString);
            if (parsed is null || parsed.Count == 0)
            {
                return Empty;
            }

            return new StringCatalog(new Dictionary<string, string>(parsed, StringComparer.Ordinal));
        }
        catch (Exception exception)
        {
            PlacardLog.Error(exception, $"Failed to load the language catalog '{path}'");
            return Empty;
        }
    }
}
