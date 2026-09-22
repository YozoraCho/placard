using Dalamud.Bindings.ImGui;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class Surfaces
{
    public static void Card(ImDrawListPtr drawList, Vector2 min, Vector2 max, float rounding, bool elevated = false)
    {
        if (elevated)
        {
            Elevation.Floating(drawList, min, max, rounding, UiScaleOf());
        }

        Squircle.Fill(drawList, min, max, rounding, ImGui.GetColorU32(PlacardTheme.CardFill));
        Squircle.Stroke(drawList, min, max, rounding, ImGui.GetColorU32(PlacardTheme.CardStroke),
            Metrics.Stroke.Hairline);
    }

    private static float UiScaleOf() => Core.UiScale.Current;
}
