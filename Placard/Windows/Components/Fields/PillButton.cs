using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class PillButton
{
    private const float LabelInset = 18f;

    public static bool Draw(Rect rect, string label, bool filled, Vector4 accent, bool overlay = false,
        bool interactive = true)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var radius = rect.Height * 0.5f;
        var hovered = interactive && (overlay
            ? UiInteract.HoverWindowOnly(rect.Min, rect.Max)
            : UiInteract.Hover(rect.Min, rect.Max));
        var fill = filled
            ? (hovered ? Palette.Lighten(accent, 0.10f) : accent)
            : hovered ? PlacardTheme.HoverTint : PlacardTheme.FieldSurface;
        var alpha = interactive ? 1f : 0.45f;
        Squircle.Fill(drawList, rect.Min, rect.Max, radius,
            ImGui.GetColorU32(Palette.WithAlpha(fill, fill.W * alpha)));
        var ink = filled ? PlacardTheme.AccentInk : PlacardTheme.TitleInk;
        var maxWidth = MathF.Max(1f, rect.Width - LabelInset * scale);
        var display = Typography.FitText(label, maxWidth, TextStyles.SubheadlineEmphasized);
        var size = Typography.Measure(display, TextStyles.SubheadlineEmphasized);
        Typography.Draw(drawList, rect.Center - size * 0.5f, display,
            Palette.WithAlpha(ink, alpha), TextStyles.SubheadlineEmphasized);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return interactive && UiInteract.Click(rect.Min, rect.Max, hovered);
    }
}
