using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class SettingsRow
{
    private const float LabelFloorFraction = 0.55f;
    private const float ColumnGap = 12f;
    private const float ChevronSize = 6f;
    private const float ChevronThickness = 2.2f;

    public static bool Bool(Rect row, string label, bool value, string? id = null, bool dimmed = false)
    {
        var scale = UiScale.Current;
        var width = Metrics.Size.ToggleWidth * scale;
        var height = Metrics.Size.ToggleHeight * scale;
        var toggleMin = new Vector2(row.Max.X - width, row.Center.Y - height * 0.5f);
        var labelMaxWidth = MathF.Max(1f, toggleMin.X - 10f * scale - row.Min.X);
        var display = Typography.FitText(label, labelMaxWidth, TextStyles.BodyEmphasized);
        var labelSize = Typography.Measure(display, TextStyles.BodyEmphasized);
        Typography.Draw(ImGui.GetWindowDrawList(), new Vector2(row.Min.X, row.Center.Y - labelSize.Y * 0.5f), display,
            dimmed ? PlacardTheme.MutedInk : PlacardTheme.TitleInk, TextStyles.BodyEmphasized);
        return Toggle.Draw(id ?? label, new Rect(toggleMin, toggleMin + new Vector2(width, height)), value);
    }

    public static void Info(Rect row, string label, string value)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var gap = ColumnGap * scale;
        var available = MathF.Max(1f, row.Width - gap);
        var valueFullWidth = Typography.Measure(value, TextStyles.Body).X;
        var labelNaturalWidth = Typography.Measure(label, TextStyles.BodyEmphasized).X;
        var labelFloor = MathF.Min(labelNaturalWidth, available * LabelFloorFraction);
        var labelCap = Math.Clamp(available - valueFullWidth, labelFloor, available);
        var labelDisplay = Typography.FitText(label, labelCap, TextStyles.BodyEmphasized);
        var labelSize = Typography.Measure(labelDisplay, TextStyles.BodyEmphasized);
        Typography.Draw(drawList, new Vector2(row.Min.X, row.Center.Y - labelSize.Y * 0.5f), labelDisplay,
            PlacardTheme.TitleInk, TextStyles.BodyEmphasized);

        var valueMaxWidth = MathF.Max(1f, available - labelSize.X);
        var valueDisplay = Typography.FitText(value, valueMaxWidth, TextStyles.Body);
        var valueSize = Typography.Measure(valueDisplay, TextStyles.Body);
        Typography.Draw(drawList, new Vector2(row.Max.X - valueSize.X, row.Center.Y - valueSize.Y * 0.5f),
            valueDisplay, PlacardTheme.MutedInk, TextStyles.Body);
    }

    public static bool Disclosure(Rect row, string label, string value, string? id = null, bool dimmed = false,
        bool interactive = true)
    {
        var scale = UiScale.Current;
        var hovered = interactive && UiInteract.Hover(row.Min, row.Max);
        if (hovered)
        {
            DrawRowHighlight(row);
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        var chevronWidth = ChevronSize * scale;
        var chevronTip = new Vector2(row.Max.X, row.Center.Y);
        var chevronGap = ColumnGap * scale;
        var valueRight = chevronTip.X - chevronWidth - chevronGap;
        DrawTwoColumnText(row, label, value, valueRight, dimmed);
        DrawChevronRight(chevronTip, chevronWidth, ChevronThickness * scale, PlacardTheme.MutedInk);
        return interactive && UiInteract.Click(row.Min, row.Max, hovered);
    }

    public static bool Action(Rect row, string label, Vector4 color, bool interactive = true)
    {
        var hovered = interactive && UiInteract.Hover(row.Min, row.Max);
        if (hovered)
        {
            DrawRowHighlight(row);
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        var display = Typography.FitText(label, row.Width, TextStyles.BodyEmphasized);
        var labelSize = Typography.Measure(display, TextStyles.BodyEmphasized);
        Typography.Draw(ImGui.GetWindowDrawList(),
            new Vector2(row.Center.X - labelSize.X * 0.5f, row.Center.Y - labelSize.Y * 0.5f), display, color,
            TextStyles.BodyEmphasized);
        return interactive && UiInteract.Click(row.Min, row.Max, hovered);
    }

    public static void DrawRowHighlight(Rect row)
    {
        var scale = UiScale.Current;
        var pressed = ImGui.IsMouseDown(ImGuiMouseButton.Left);
        var min = new Vector2(row.Min.X - 10f * scale, row.Min.Y + 3f * scale);
        var max = new Vector2(row.Max.X + 10f * scale, row.Max.Y - 3f * scale);
        var alpha = pressed ? 0.10f : 0.05f;
        Squircle.Fill(ImGui.GetWindowDrawList(), min, max, Metrics.Radius.Sm * scale,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.TitleInk, PlacardTheme.TitleInk.W * alpha)));
    }

    private static void DrawTwoColumnText(Rect row, string label, string value, float valueRight, bool dimmed)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var gap = ColumnGap * scale;
        var available = MathF.Max(1f, valueRight - row.Min.X);
        var labelCap = value.Length == 0
            ? available
            : MathF.Max(1f, available - gap - Typography.Measure(value, TextStyles.Body).X);
        var labelDisplay = Typography.FitText(label, labelCap, TextStyles.BodyEmphasized);
        var labelSize = Typography.Measure(labelDisplay, TextStyles.BodyEmphasized);
        Typography.Draw(drawList, new Vector2(row.Min.X, row.Center.Y - labelSize.Y * 0.5f), labelDisplay,
            dimmed ? PlacardTheme.MutedInk : PlacardTheme.TitleInk, TextStyles.BodyEmphasized);

        if (value.Length == 0)
        {
            return;
        }

        var valueMaxWidth = MathF.Max(1f, valueRight - row.Min.X - labelSize.X - gap);
        var valueDisplay = Typography.FitText(value, valueMaxWidth, TextStyles.Body);
        var valueSize = Typography.Measure(valueDisplay, TextStyles.Body);
        Typography.Draw(drawList, new Vector2(valueRight - valueSize.X, row.Center.Y - valueSize.Y * 0.5f),
            valueDisplay, PlacardTheme.MutedInk, TextStyles.Body);
    }

    private static void DrawChevronRight(Vector2 tip, float size, float thickness, Vector4 color)
    {
        var drawList = ImGui.GetWindowDrawList();
        var packed = ImGui.GetColorU32(color);
        drawList.AddLine(new Vector2(tip.X - size, tip.Y - size), tip, packed, thickness);
        drawList.AddLine(tip, new Vector2(tip.X - size, tip.Y + size), packed, thickness);
    }
}
