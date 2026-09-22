using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Components;

internal static class Typography
{
    private const string Ellipsis = "…";
    private const float DefaultLineSpacing = 1.25f;
    private const int CacheCapacity = 512;

    private static readonly Dictionary<(string Text, float MaxWidth, float FontSize), string> FitCache = new();
    private static readonly Dictionary<(string Text, float MaxWidth, float FontSize), string[]> WrapCache = new();
    private static readonly List<string> WrapScratch = new();
    private static readonly string[] NoLines = [];

    private static float cachedFontSize;

    public static float BaseSize => ImGui.GetFontSize();

    public static float SizeOf(in TextStyle style) => ImGui.GetFontSize() * style.Scale;

    public static Vector2 Measure(string text, in TextStyle style) => ImGui.CalcTextSize(text) * style.Scale;

    public static float LineHeight(in TextStyle style) => ImGui.GetTextLineHeight() * style.Scale;

    public static float LineHeightWithSpacing(in TextStyle style) =>
        ImGui.GetTextLineHeightWithSpacing() * style.Scale;

    public static void Draw(ImDrawListPtr drawList, Vector2 position, string text, Vector4 color,
        in TextStyle style)
    {
        if (text.Length == 0)
        {
            return;
        }

        drawList.AddText(ImGui.GetFont(), SizeOf(style), position, ImGui.GetColorU32(color), text);
    }

    public static void DrawCentered(ImDrawListPtr drawList, Vector2 center, string text, Vector4 color,
        in TextStyle style)
    {
        if (text.Length == 0)
        {
            return;
        }

        var size = Measure(text, style);
        Draw(drawList, center - size * 0.5f, text, color, style);
    }

    public static void Draw(Vector2 position, string text, Vector4 color, in TextStyle style) =>
        Draw(ImGui.GetWindowDrawList(), position, text, color, style);

    public static void DrawCentered(Vector2 center, string text, Vector4 color, in TextStyle style) =>
        DrawCentered(ImGui.GetWindowDrawList(), center, text, color, style);

    public static float DrawWrappedLeft(Vector2 topLeft, string text, Vector4 color, in TextStyle style,
        float maxWidth) =>
        DrawWrappedLeft(ImGui.GetWindowDrawList(), topLeft, text, color, style, maxWidth);

    public static float DrawWrappedCentered(ImDrawListPtr drawList, Vector2 topCenter, string text, Vector4 color,
        in TextStyle style, float maxWidth, float lineSpacing = DefaultLineSpacing) =>
        DrawWrappedCentered(drawList, text, style, color, topCenter, maxWidth, lineSpacing);

    public static float DrawWrappedCentered(Vector2 topCenter, string text, Vector4 color, in TextStyle style,
        float maxWidth, float lineSpacing = DefaultLineSpacing) =>
        DrawWrappedCentered(ImGui.GetWindowDrawList(), text, style, color, topCenter, maxWidth, lineSpacing);

    public static string FitText(string text, float maxWidth, in TextStyle style)
    {
        if (maxWidth <= 0f || text.Length == 0)
        {
            return text;
        }

        InvalidateOnFontChange();
        var scaledWidth = maxWidth / style.Scale;
        var key = (text, scaledWidth, cachedFontSize);
        if (FitCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (FitCache.Count >= CacheCapacity)
        {
            FitCache.Clear();
        }

        var result = Shorten(text, scaledWidth);
        FitCache[key] = result;
        return result;
    }

    public static Vector2 MeasureWrappedBlock(string text, in TextStyle style, float maxWidth)
    {
        var lines = WrapLines(text, maxWidth / style.Scale);
        if (lines.Length == 0)
        {
            return Vector2.Zero;
        }

        var width = 0f;
        for (var index = 0; index < lines.Length; index++)
        {
            width = MathF.Max(width, ImGui.CalcTextSize(lines[index]).X);
        }

        var spacing = ImGui.GetTextLineHeightWithSpacing();
        var block = (lines.Length - 1) * spacing + ImGui.GetTextLineHeight();
        return new Vector2(width, block) * style.Scale;
    }

    public static float DrawWrappedLeft(ImDrawListPtr drawList, Vector2 topLeft, string text, Vector4 color,
        in TextStyle style, float maxWidth)
    {
        var lines = WrapLines(text, maxWidth / style.Scale);
        if (lines.Length == 0)
        {
            return 0f;
        }

        var font = ImGui.GetFont();
        var fontSize = SizeOf(style);
        var lineHeight = LineHeightWithSpacing(style);
        var packed = ImGui.GetColorU32(color);
        for (var index = 0; index < lines.Length; index++)
        {
            if (lines[index].Length == 0)
            {
                continue;
            }

            drawList.AddText(font, fontSize, new Vector2(topLeft.X, topLeft.Y + index * lineHeight), packed,
                lines[index]);
        }

        return (lines.Length - 1) * lineHeight + LineHeight(style);
    }

    public static float DrawWrappedCentered(ImDrawListPtr drawList, string text, in TextStyle style, Vector4 color,
        Vector2 topCenter, float maxWidth, float lineSpacing = DefaultLineSpacing)
    {
        var lines = WrapLines(text, maxWidth / style.Scale);
        if (lines.Length == 0)
        {
            return topCenter.Y;
        }

        var font = ImGui.GetFont();
        var fontSize = SizeOf(style);
        var packed = ImGui.GetColorU32(color);
        var lineHeight = ImGui.CalcTextSize("Ay").Y * style.Scale * lineSpacing;
        var y = topCenter.Y;
        for (var index = 0; index < lines.Length; index++)
        {
            var line = lines[index];
            if (line.Length == 0)
            {
                y += lineHeight;
                continue;
            }

            var size = ImGui.CalcTextSize(line) * style.Scale;
            drawList.AddText(font, fontSize,
                new Vector2(topCenter.X - size.X * 0.5f, y + (lineHeight - size.Y) * 0.5f), packed, line);
            y += lineHeight;
        }

        return y;
    }

    private static string Shorten(string text, float maxWidth)
    {
        if (ImGui.CalcTextSize(text).X <= maxWidth)
        {
            return text;
        }

        for (var length = text.Length - 1; length > 0; length--)
        {
            var candidate = string.Concat(text.AsSpan(0, length).TrimEnd(), Ellipsis.AsSpan());
            if (ImGui.CalcTextSize(candidate).X <= maxWidth)
            {
                return candidate;
            }
        }

        return Ellipsis;
    }

    private static string[] WrapLines(string text, float maxWidth)
    {
        if (text.Length == 0)
        {
            return NoLines;
        }

        InvalidateOnFontChange();
        var key = (text, MathF.Floor(maxWidth), cachedFontSize);
        if (WrapCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        if (WrapCache.Count >= CacheCapacity)
        {
            WrapCache.Clear();
        }

        var lines = BuildWrappedLines(text, maxWidth);
        WrapCache[key] = lines;
        return lines;
    }

    private static string[] BuildWrappedLines(string text, float maxWidth)
    {
        WrapScratch.Clear();
        var paragraphStart = 0;
        while (paragraphStart <= text.Length)
        {
            var breakIndex = text.IndexOf('\n', paragraphStart);
            var paragraphEnd = breakIndex < 0 ? text.Length : breakIndex;
            AppendWrapped(text.AsSpan(paragraphStart, paragraphEnd - paragraphStart).TrimEnd('\r'), maxWidth);
            if (breakIndex < 0)
            {
                break;
            }

            paragraphStart = breakIndex + 1;
        }

        return WrapScratch.ToArray();
    }

    private static void AppendWrapped(ReadOnlySpan<char> paragraph, float maxWidth)
    {
        if (paragraph.Length == 0)
        {
            WrapScratch.Add(string.Empty);
            return;
        }

        var lineStart = 0;
        var lineEnd = 0;
        var cursor = 0;
        while (cursor < paragraph.Length)
        {
            var spaceOffset = paragraph[cursor..].IndexOf(' ');
            var wordEnd = spaceOffset < 0 ? paragraph.Length : cursor + spaceOffset;
            if (lineEnd > lineStart && WidthOf(paragraph[lineStart..wordEnd]) > maxWidth)
            {
                WrapScratch.Add(paragraph[lineStart..lineEnd].ToString());
                lineStart = lineEnd;
                while (lineStart < paragraph.Length && paragraph[lineStart] == ' ')
                {
                    lineStart++;
                }

                lineEnd = lineStart;
                cursor = lineStart;
                continue;
            }

            lineEnd = wordEnd;
            cursor = wordEnd + 1;
        }

        if (lineEnd > lineStart)
        {
            WrapScratch.Add(paragraph[lineStart..lineEnd].ToString());
        }
    }

    private static float WidthOf(ReadOnlySpan<char> text) => ImGui.CalcTextSize(text.ToString()).X;

    private static void InvalidateOnFontChange()
    {
        var fontSize = ImGui.GetFontSize();
        if (fontSize == cachedFontSize)
        {
            return;
        }

        cachedFontSize = fontSize;
        FitCache.Clear();
        WrapCache.Clear();
    }
}
