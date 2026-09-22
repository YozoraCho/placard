using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Components;

internal static class BackButton
{
    public static bool Draw(Vector2 center, float radius, Vector4 chevronInk, bool hovered, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var reach = radius * 0.5f;
        var thickness = 2.4f * scale;
        var tip = new Vector2(center.X - reach * 0.4f, center.Y);
        var upper = new Vector2(tip.X + reach, tip.Y - reach);
        var lower = new Vector2(tip.X + reach, tip.Y + reach);
        var ink = ImGui.GetColorU32(hovered ? chevronInk : chevronInk with { W = chevronInk.W * 0.88f });
        drawList.AddLine(upper, tip, ink, thickness);
        drawList.AddLine(tip, lower, ink, thickness);
        var cap = thickness * 0.5f;
        drawList.AddCircleFilled(upper, cap, ink, 8);
        drawList.AddCircleFilled(tip, cap, ink, 8);
        drawList.AddCircleFilled(lower, cap, ink, 8);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        var hit = new Vector2(radius, radius);
        return UiInteract.Click(center - hit, center + hit, hovered);
    }
}
