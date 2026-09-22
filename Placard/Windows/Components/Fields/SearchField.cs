using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class SearchField
{
    private const float SideInset = 12f;
    private const float VerticalInset = 6f;
    private const float IconInset = 16f;
    private const float TextInset = 34f;
    private const float ClearInset = 18f;
    private const float IconScale = 0.8f;

    public static void Draw(Rect bar, string imguiId, string hint, ref string text, int maxLength = 100)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var pillMin = new Vector2(bar.Min.X + SideInset * scale, bar.Min.Y + VerticalInset * scale);
        var pillMax = new Vector2(bar.Max.X - SideInset * scale, bar.Max.Y - VerticalInset * scale);
        var centerY = (pillMin.Y + pillMax.Y) * 0.5f;
        Squircle.Fill(drawList, pillMin, pillMax, (pillMax.Y - pillMin.Y) * 0.5f,
            ImGui.GetColorU32(PlacardTheme.FieldSurface));
        Icons.DrawCentered(drawList, new Vector2(pillMin.X + IconInset * scale, centerY), FontAwesomeIcon.Search,
            PlacardTheme.MutedInk, IconScale);

        var hasText = text.Length > 0;
        var trailing = hasText ? ClearInset + IconInset : IconInset;
        ImGui.SetCursorScreenPos(new Vector2(pillMin.X + TextInset * scale,
            centerY - ImGui.GetFrameHeight() * 0.5f));
        ImGui.SetNextItemWidth(MathF.Max(1f, pillMax.X - pillMin.X - (TextInset + trailing) * scale));
        using (ImRaii.PushColor(ImGuiCol.FrameBg, PlacardTheme.Transparent))
        using (ImRaii.PushColor(ImGuiCol.Text, PlacardTheme.TitleInk))
        {
            ImGui.InputTextWithHint(imguiId, hint, ref text, maxLength);
        }

        if (!hasText)
        {
            return;
        }

        var clearCenter = new Vector2(pillMax.X - ClearInset * scale, centerY);
        var clearRadius = 8f * scale;
        var clearHit = new Vector2(clearRadius, clearRadius);
        var clearHovered = UiInteract.Hover(clearCenter - clearHit, clearCenter + clearHit);
        Icons.DrawCentered(drawList, clearCenter, FontAwesomeIcon.TimesCircle,
            clearHovered ? PlacardTheme.TitleInk : PlacardTheme.MutedInk, IconScale);
        if (clearHovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        if (UiInteract.Click(clearCenter - clearHit, clearCenter + clearHit, clearHovered))
        {
            text = string.Empty;
        }
    }
}
