using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Placard.Core;

namespace Placard.Windows.Components;

internal static class Icons
{
    public static void DrawCentered(ImDrawListPtr drawList, Vector2 center, FontAwesomeIcon icon, Vector4 color,
        float scale)
    {
        var glyph = icon.ToIconString();
        float fontSize;
        Vector2 size;
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            fontSize = ImGui.GetFontSize() * scale;
            size = ImGui.CalcTextSize(glyph) * scale;
        }

        var origin = center - size * 0.5f + OpticalOffset(icon) * UiScale.Current;
        drawList.AddText(UiBuilder.IconFont, fontSize, origin, ImGui.GetColorU32(color), glyph, 0f);
    }

    private static Vector2 OpticalOffset(FontAwesomeIcon icon) => icon switch
    {
        FontAwesomeIcon.Cog => new Vector2(1.5f, 0f),
        _ => Vector2.Zero,
    };
}
