using Placard.Core;
using Placard.Core.Animation;
using Placard.Core.Housing;
using Placard.Core.Localization;
using Placard.Core.Theme;
using Placard.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;

namespace Placard.Windows.Housing;

internal static class HousingChrome
{
    public const float ChipHeight = 22f;
    public const float StatRowHeight = 26f;

    private static readonly Vector4 White = new(1f, 1f, 1f, 1f);
    private static readonly Vector4 Transparent = new(0f, 0f, 0f, 0f);

    public static bool Hover(Vector2 min, Vector2 max, bool overlay) =>
        overlay ? UiInteract.HoverWindowOnly(min, max) : UiInteract.Hover(min, max);

    public static float SelectorHeight(float scale) =>
        Typography.LineHeight(TextStyles.Caption2) + Typography.LineHeight(TextStyles.SubheadlineEmphasized) +
        11f * scale;
    public static bool Selector(Rect rect, string label, string value, bool overlay = false)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var hovered = Hover(rect.Min, rect.Max, overlay);
        var rounding = Metrics.Radius.Sm * scale;
        Squircle.Fill(drawList, rect.Min, rect.Max, rounding,
            ImGui.GetColorU32(hovered ? PlacardTheme.HoverTint : PlacardTheme.FieldSurface));
        Squircle.Stroke(drawList, rect.Min, rect.Max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Accent, hovered ? 0.55f : 0.22f)), Metrics.Stroke.Hairline);
        var caretWidth = 13f * scale;
        var left = rect.Min.X + 9f * scale;
        var textWidth = MathF.Max(1f, rect.Width - 17f * scale - caretWidth);
        var labelStyle = TextStyles.Caption2;
        var valueStyle = TextStyles.SubheadlineEmphasized;
        var labelHeight = Typography.LineHeight(labelStyle);
        var valueHeight = Typography.LineHeight(valueStyle);
        var stackHeight = labelHeight + valueHeight + 1f * scale;
        var top = rect.Center.Y - stackHeight * 0.5f;
        Typography.Draw(drawList, new Vector2(left, top),
            Typography.FitText(Loc.Culture.TextInfo.ToUpper(label), textWidth, labelStyle), PlacardTheme.MutedInk, labelStyle);
        Typography.Draw(drawList, new Vector2(left, top + labelHeight + 1f * scale),
            Typography.FitText(value, textWidth, valueStyle), PlacardTheme.TitleInk, valueStyle);
        Caret(drawList, new Vector2(rect.Max.X - caretWidth * 0.62f, rect.Center.Y), 3.6f * scale, 1.5f * scale,
            hovered ? PlacardTheme.Accent : PlacardTheme.MutedInk);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return UiInteract.Click(rect.Min, rect.Max, hovered);
    }

    public static void Chip(ImDrawListPtr drawList, Vector2 topLeft, string label, Vector4 hue, bool solid)
    {
        var scale = UiScale.Current;
        var height = ChipHeight * scale;
        var width = MeasureChip(label);
        var min = topLeft;
        var max = new Vector2(topLeft.X + width, topLeft.Y + height);
        var radius = height * 0.5f;
        if (solid)
        {
            Squircle.Fill(drawList, min, max, radius, ImGui.GetColorU32(Palette.WithAlpha(hue, 0.95f)));
        }
        else
        {
            Squircle.Fill(drawList, min, max, radius, ImGui.GetColorU32(Palette.WithAlpha(hue, 0.16f)));
            Squircle.Stroke(drawList, min, max, radius, ImGui.GetColorU32(Palette.WithAlpha(hue, 0.42f)),
                Metrics.Stroke.Hairline);
        }

        var ink = solid
            ? Palette.Luminance(hue) > 0.62f ? PlacardTheme.AccentInk : White
            : Palette.Mix(hue, White, 0.55f);
        Typography.DrawCentered(drawList, new Vector2((min.X + max.X) * 0.5f, (min.Y + max.Y) * 0.5f), label, ink,
            TextStyles.Caption1);
    }

    public static float MeasureChip(string label) =>
        Typography.Measure(label, TextStyles.Caption1).X + 16f * UiScale.Current;

    public static Vector4 FreshnessHue(HousingDataFreshness freshness, Vector4 accent) => freshness switch
    {
        HousingDataFreshness.Live => accent,
        HousingDataFreshness.Recent => PlacardTheme.Brass,
        HousingDataFreshness.Stale => PlacardTheme.Results,
        HousingDataFreshness.Cached => PlacardTheme.Parchment,
        _ => PlacardTheme.Closed,
    };

    public static void StatRow(ImDrawListPtr drawList, Rect row, string label, string value,
        bool emphasise = false)
    {
        var scale = UiScale.Current;
        var labelStyle = TextStyles.Footnote;
        var valueStyle = emphasise ? TextStyles.SubheadlineEmphasized : TextStyles.Subheadline;
        var labelWidth = MathF.Min(row.Width * 0.52f, Typography.Measure(label, labelStyle).X);
        Typography.Draw(drawList,
            new Vector2(row.Min.X, row.Center.Y - Typography.LineHeight(labelStyle) * 0.5f),
            Typography.FitText(label, labelWidth, labelStyle), PlacardTheme.MutedInk, labelStyle);
        var valueMax = MathF.Max(1f, row.Width - labelWidth - 10f * scale);
        var fitted = Typography.FitText(value, valueMax, valueStyle);
        var valueSize = Typography.Measure(fitted, valueStyle);
        Typography.Draw(drawList, new Vector2(row.Max.X - valueSize.X, row.Center.Y - valueSize.Y * 0.5f), fitted,
            emphasise ? PlacardTheme.TitleInk : PlacardTheme.BodyInk, valueStyle);
    }

    public static bool PillButton(Rect rect, string label, bool filled, bool overlay = false,
        bool enabled = true)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var hovered = enabled && Hover(rect.Min, rect.Max, overlay);
        var radius = rect.Height * 0.5f;
        var accent = PlacardTheme.Accent;
        var fill = filled
            ? enabled
                ? hovered ? Palette.Mix(accent, White, 0.14f) : accent
                : Palette.WithAlpha(accent, 0.38f)
            : enabled
                ? hovered ? PlacardTheme.HoverTint : PlacardTheme.FieldSurface
                : Palette.WithAlpha(PlacardTheme.FieldSurface, PlacardTheme.FieldSurface.W * 0.5f);
        Squircle.Fill(drawList, rect.Min, rect.Max, radius, ImGui.GetColorU32(fill));
        if (!filled)
        {
            Squircle.Stroke(drawList, rect.Min, rect.Max, radius,
                ImGui.GetColorU32(Palette.WithAlpha(White, enabled ? 0.16f : 0.08f)), Metrics.Stroke.Hairline);
        }

        var ink = filled
            ? PlacardTheme.AccentInk
            : enabled
                ? hovered ? PlacardTheme.TitleInk : PlacardTheme.BodyInk
                : PlacardTheme.MutedInk;
        var labelMaxWidth = MathF.Max(1f, rect.Width - rect.Height - 6f * scale);
        var style = TextStyles.SubheadlineEmphasized;
        var fitted = Typography.FitText(label, labelMaxWidth, style);
        Typography.DrawCentered(drawList, rect.Center, fitted, ink, style);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return enabled && UiInteract.Click(rect.Min, rect.Max, hovered);
    }

    public static float MeasurePill(string label, float height) =>
        Typography.Measure(label, TextStyles.SubheadlineEmphasized).X + height + 14f * UiScale.Current;

    public static void LayoutPills(Rect row, ReadOnlySpan<string> labels, float gap, Span<Rect> into)
    {
        if (labels.Length == 0 || into.Length < labels.Length)
        {
            return;
        }

        var available = row.Width - gap * (labels.Length - 1);
        var total = 0f;
        Span<float> widths = stackalloc float[labels.Length];
        for (var index = 0; index < labels.Length; index++)
        {
            widths[index] = MeasurePill(labels[index], row.Height);
            total += widths[index];
        }

        var factor = total > 0f ? available / total : 1f;
        var x = row.Min.X;
        for (var index = 0; index < labels.Length; index++)
        {
            var width = index == labels.Length - 1 ? row.Max.X - x : widths[index] * factor;
            into[index] = new Rect(new Vector2(x, row.Min.Y), new Vector2(x + width, row.Max.Y));
            x += width + gap;
        }
    }

    public static bool DangerPillButton(Rect rect, string label, bool overlay = false)
    {
        var drawList = ImGui.GetWindowDrawList();
        var hovered = Hover(rect.Min, rect.Max, overlay);
        var radius = rect.Height * 0.5f;
        var danger = PlacardTheme.Danger;
        if (hovered)
        {
            Squircle.Fill(drawList, rect.Min, rect.Max, radius,
                ImGui.GetColorU32(Palette.WithAlpha(danger, 0.18f)));
        }

        Squircle.Stroke(drawList, rect.Min, rect.Max, radius,
            ImGui.GetColorU32(Palette.WithAlpha(danger, 0.55f)), Metrics.Stroke.Thin);
        Typography.DrawCentered(drawList, rect.Center, label, Palette.Mix(danger, White, 0.2f),
            TextStyles.SubheadlineEmphasized);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return UiInteract.Click(rect.Min, rect.Max, hovered);
    }

    public static int Segment(Rect rect, string first, string second, int selected, bool overlay = false)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var radius = rect.Height * 0.5f;
        Squircle.Fill(drawList, rect.Min, rect.Max, radius, ImGui.GetColorU32(PlacardTheme.ControlTrack));
        Squircle.Stroke(drawList, rect.Min, rect.Max, radius, ImGui.GetColorU32(PlacardTheme.CardStroke),
            Metrics.Stroke.Hairline * scale);
        var half = rect.Width * 0.5f;
        var inset = 2f * scale;
        var thumbMin = new Vector2(rect.Min.X + inset + (selected == 1 ? half : 0f), rect.Min.Y + inset);
        var thumbMax = new Vector2(thumbMin.X + half - inset * 2f, rect.Max.Y - inset);
        Squircle.Fill(drawList, thumbMin, thumbMax, radius - inset, ImGui.GetColorU32(PlacardTheme.Accent));
        var result = selected;
        for (var index = 0; index < 2; index++)
        {
            var min = new Vector2(rect.Min.X + index * half, rect.Min.Y);
            var max = new Vector2(min.X + half, rect.Max.Y);
            var hovered = Hover(min, max, overlay);
            var active = index == selected;
            if (hovered && !active)
            {
                var hoverMin = new Vector2(min.X + inset, min.Y + inset);
                var hoverMax = new Vector2(max.X - inset, max.Y - inset);
                Squircle.Fill(drawList, hoverMin, hoverMax, radius - inset,
                    ImGui.GetColorU32(PlacardTheme.ControlTrackHover));
            }

            var ink = active ? PlacardTheme.AccentInk : hovered ? PlacardTheme.TitleInk : PlacardTheme.BodyInk;
            var label = index == 0 ? first : second;
            Typography.DrawCentered(drawList, new Vector2((min.X + max.X) * 0.5f, rect.Center.Y),
                Typography.FitText(label, half - 12f * scale, TextStyles.SubheadlineEmphasized), ink,
                TextStyles.SubheadlineEmphasized);
            if (hovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            if (UiInteract.Click(min, max, hovered))
            {
                result = index;
            }
        }

        return result;
    }

    public static bool MapButton(Vector2 center, float radius, FontAwesomeIcon icon, string tooltip,
        bool active = false, bool overlay = false)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var hit = new Vector2(radius, radius);
        var hovered = Hover(center - hit, center + hit, overlay);
        var fill = active
            ? Palette.WithAlpha(PlacardTheme.Accent, 0.92f)
            : hovered ? PlacardTheme.ControlTrackHover : PlacardTheme.ControlTrack;
        Elevation.Icon(drawList, center - hit, center + hit, radius, radius, 0.8f);
        drawList.AddCircleFilled(center, radius, ImGui.GetColorU32(fill), 28);
        drawList.AddCircle(center, radius, ImGui.GetColorU32(Palette.WithAlpha(White, hovered ? 0.30f : 0.16f)), 28,
            1f * scale);
        var ink = active ? PlacardTheme.AccentInk : hovered ? PlacardTheme.TitleInk : PlacardTheme.BodyInk;
        Icons.DrawCentered(drawList, center, icon, ink, 0.78f);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        HoverTooltip.Show(new Rect(center - hit, center + hit), tooltip, HoverLabelSide.Above);
        return UiInteract.Click(center - hit, center + hit, hovered);
    }

    public static bool RefreshButton(Vector2 center, float radius, bool busy, string tooltip,
        bool overlay = false)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var hit = new Vector2(radius, radius);
        var hovered = !busy && Hover(center - hit, center + hit, overlay);
        var fill = hovered ? PlacardTheme.ControlTrackHover : PlacardTheme.ControlTrack;
        Elevation.Icon(drawList, center - hit, center + hit, radius, radius, 0.8f);
        drawList.AddCircleFilled(center, radius, ImGui.GetColorU32(fill), 28);
        drawList.AddCircle(center, radius, ImGui.GetColorU32(Palette.WithAlpha(White, hovered ? 0.30f : 0.16f)), 28,
            1f * scale);
        var ink = busy ? PlacardTheme.Accent : hovered ? PlacardTheme.TitleInk : PlacardTheme.BodyInk;
        var rotation = busy ? Pulse.Phase(900.0) * MathF.Tau : 0f;
        HousingGlyphs.RefreshArrow(drawList, center, radius * 0.50f, ink, 1.7f * scale, rotation);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        HoverTooltip.Show(new Rect(center - hit, center + hit), tooltip, HoverLabelSide.Above);
        return !busy && UiInteract.Click(center - hit, center + hit, hovered);
    }

    public static bool CloseButton(Vector2 center, float radius, bool overlay = false)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var hit = new Vector2(radius, radius);
        var hovered = Hover(center - hit, center + hit, overlay);
        drawList.AddCircleFilled(center, radius, ImGui.GetColorU32(hovered ? PlacardTheme.HoverTint : PlacardTheme.FieldSurface), 24);
        var ink = ImGui.GetColorU32(hovered ? PlacardTheme.TitleInk : PlacardTheme.MutedInk);
        var arm = radius * 0.42f;
        drawList.AddLine(center - new Vector2(arm, arm), center + new Vector2(arm, arm), ink, 1.7f * scale);
        drawList.AddLine(center + new Vector2(-arm, arm), center + new Vector2(arm, -arm), ink, 1.7f * scale);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return UiInteract.Click(center - hit, center + hit, hovered);
    }

    private static readonly Dictionary<string, string> NumberBuffers = new(StringComparer.Ordinal);

    public static int NumberStepper(Rect rect, string id, int value, int minimum, int maximum, int step, string suffix,
        bool overlay = false)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetWindowDrawList();
        var buttonRadius = rect.Height * 0.5f;
        var minusCenter = new Vector2(rect.Min.X + buttonRadius, rect.Center.Y);
        var plusCenter = new Vector2(rect.Max.X - buttonRadius, rect.Center.Y);
        var fieldMin = new Vector2(minusCenter.X + buttonRadius + 5f * scale, rect.Min.Y);
        var fieldMax = new Vector2(plusCenter.X - buttonRadius - 5f * scale, rect.Max.Y);
        var result = value;
        if (RoundButton(drawList, minusCenter, buttonRadius, "-", value > minimum, overlay, scale))
        {
            result = value - step;
        }

        if (RoundButton(drawList, plusCenter, buttonRadius, "+", value < maximum, overlay, scale))
        {
            result = value + step;
        }

        Squircle.Fill(drawList, fieldMin, fieldMax, Metrics.Radius.Sm * scale, ImGui.GetColorU32(PlacardTheme.FieldSurface));
        var typing = NumberBuffers.TryGetValue(id, out var buffer) && buffer is not null;
        var text = typing ? buffer! : value.ToString(Loc.Culture);
        var suffixWidth = suffix.Length > 0
            ? Typography.Measure(suffix, TextStyles.Caption1).X + 4f * scale
            : 0f;
        var inputWidth = MathF.Max(18f * scale, fieldMax.X - fieldMin.X - 10f * scale - suffixWidth);
        ImGui.SetCursorScreenPos(new Vector2(fieldMin.X + 5f * scale,
            rect.Center.Y - ImGui.GetFrameHeight() * 0.5f));
        ImGui.SetNextItemWidth(inputWidth);
        using (ImRaii.PushColor(ImGuiCol.FrameBg, Transparent))
        using (ImRaii.PushColor(ImGuiCol.Text, PlacardTheme.TitleInk))
        {
            ImGui.InputText(id, ref text, 6,
                ImGuiInputTextFlags.CharsDecimal | ImGuiInputTextFlags.AutoSelectAll);
        }

        if (ImGui.IsItemActive())
        {
            NumberBuffers[id] = text;
        }
        else if (typing)
        {
            NumberBuffers.Remove(id);
            if (int.TryParse(text, System.Globalization.NumberStyles.Integer, Loc.Culture, out var typed))
            {
                result = typed;
            }
        }

        if (suffix.Length > 0)
        {
            var suffixSize = Typography.Measure(suffix, TextStyles.Caption1);
            Typography.Draw(drawList, new Vector2(fieldMax.X - 5f * scale - suffixSize.X,
                rect.Center.Y - suffixSize.Y * 0.5f), suffix, PlacardTheme.MutedInk, TextStyles.Caption1);
        }

        return Math.Clamp(result, minimum, maximum);
    }

    private static bool RoundButton(ImDrawListPtr drawList, Vector2 center, float radius, string glyph,
        bool enabled, bool overlay, float scale)
    {
        var hit = new Vector2(radius, radius);
        var hovered = enabled && Hover(center - hit, center + hit, overlay);
        drawList.AddCircleFilled(center, radius,
            ImGui.GetColorU32(hovered ? Palette.WithAlpha(PlacardTheme.Accent, 0.85f) : PlacardTheme.FieldSurface), 24);
        var ink = hovered
            ? PlacardTheme.AccentInk
            : enabled
                ? PlacardTheme.TitleInk
                : PlacardTheme.MutedInk;
        var arm = radius * 0.40f;
        var packed = ImGui.GetColorU32(ink);
        drawList.AddLine(new Vector2(center.X - arm, center.Y), new Vector2(center.X + arm, center.Y), packed,
            1.8f * scale);
        if (glyph == "+")
        {
            drawList.AddLine(new Vector2(center.X, center.Y - arm), new Vector2(center.X, center.Y + arm), packed,
                1.8f * scale);
        }

        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        return enabled && UiInteract.Click(center - hit, center + hit, hovered);
    }

    public static void SectionLabel(ImDrawListPtr drawList, Vector2 topLeft, float width, string label)
    {
        var scale = UiScale.Current;
        var text = Loc.Culture.TextInfo.ToUpper(label);
        Typography.Draw(drawList, topLeft, Typography.FitText(text, width, TextStyles.Caption1), PlacardTheme.HeaderInk,
            TextStyles.Caption1);
        var ruleY = topLeft.Y + Typography.LineHeight(TextStyles.Caption1) + 3f * scale;
        drawList.AddLine(new Vector2(topLeft.X, ruleY), new Vector2(topLeft.X + width, ruleY),
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Brass, 0.24f)), 1f * scale);
    }

    public static void Caret(ImDrawListPtr drawList, Vector2 center, float size, float thickness, Vector4 color)
    {
        var packed = ImGui.GetColorU32(color);
        var tip = new Vector2(center.X, center.Y + size * 0.62f);
        drawList.AddLine(new Vector2(center.X - size, center.Y - size * 0.35f), tip, packed, thickness);
        drawList.AddLine(tip, new Vector2(center.X + size, center.Y - size * 0.35f), packed, thickness);
    }

    public static void SheetChrome(ImDrawListPtr drawList, Rect sheet, Rect behind, float progress)
    {
        var scale = UiScale.Current;
        Veil.Draw(drawList, behind.Min, behind.Max, 0.30f * progress);
        var rounding = Metrics.Radius.Lg * scale;
        Elevation.Floating(drawList, sheet.Min, sheet.Max, rounding, scale, progress);
        Squircle.Fill(drawList, sheet.Min, sheet.Max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.SurfaceMuted, 0.985f * progress)));
        Squircle.Stroke(drawList, sheet.Min, sheet.Max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Accent, 0.22f * progress)),
            Metrics.Stroke.Hairline * scale);
        var handleWidth = 34f * scale;
        var handleY = sheet.Min.Y + 7f * scale;
        drawList.AddLine(new Vector2(sheet.Center.X - handleWidth * 0.5f, handleY),
            new Vector2(sheet.Center.X + handleWidth * 0.5f, handleY),
            ImGui.GetColorU32(Palette.WithAlpha(White, 0.24f * progress)), 3f * scale);
    }
}
