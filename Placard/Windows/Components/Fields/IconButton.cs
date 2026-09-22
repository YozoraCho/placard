using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class IconButton
{
    public static bool Draw(Vector2 center, float hitRadius, FontAwesomeIcon icon, Vector4 color, Vector4 background,
        float glyphScale, string tooltip = "", HoverLabelSide tooltipSide = HoverLabelSide.Above)
    {
        var drawList = ImGui.GetWindowDrawList();
        var hit = new Vector2(hitRadius, hitRadius);
        var bounds = new Rect(center - hit, center + hit);
        var hovered = UiInteract.Hover(bounds.Min, bounds.Max);
        if (background.W > 0f)
        {
            drawList.AddCircleFilled(center, hitRadius,
                ImGui.GetColorU32(hovered ? Palette.Lighten(background, 0.08f) : background), 24);
        }

        Icons.DrawCentered(drawList, center, icon,
            hovered ? Palette.Lighten(color, 0.20f) : color, glyphScale);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        HoverTooltip.Show(bounds, tooltip, tooltipSide);
        return UiInteract.Click(bounds.Min, bounds.Max, hovered);
    }
}
