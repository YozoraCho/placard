using Dalamud.Bindings.ImGui;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class PopoverSurface
{
    public static void Draw(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, float scale,
        float alpha = 1f)
    {
        Elevation.Floating(drawList, min, max, rounding, scale, alpha);
        Squircle.Fill(drawList, min, max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.SurfaceMuted, 0.98f * alpha)));
        Squircle.Stroke(drawList, min, max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.CardStroke, PlacardTheme.CardStroke.W * alpha)), Metrics.Stroke.Hairline * scale);
    }
}
