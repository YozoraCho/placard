using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class Selection
{
    public const float SmoothTime = 0.11f;
    public const float RowHeight = 34f;
    public const float PanelPadding = 16f;
    public const float TitleGap = 14f;
    public const float RowInset = 12f;
    public const float MarkerInset = 14f;
    public const float MarkerRadius = 3.2f;

    public static readonly TextStyle RowStyle = TextStyles.Subheadline;
    public static readonly TextStyle TitleStyle = TextStyles.SubheadlineEmphasized;
    public static readonly TextStyle LegendStyle = TextStyles.Caption1;

    public static float Radius(float scale) => Metrics.Radius.Sm * scale;

    public static void Backdrop(ImDrawListPtr drawList, Rect panel, float scale, float alpha = 1f)
    {
        var radius = Metrics.Radius.Card * scale;
        Elevation.Floating(drawList, panel.Min, panel.Max, radius, scale, alpha);
        Squircle.Fill(drawList, panel.Min, panel.Max, radius,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.SurfaceMuted, PlacardTheme.SurfaceMuted.W * alpha)));
        Squircle.Stroke(drawList, panel.Min, panel.Max, radius,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.CardStroke, PlacardTheme.CardStroke.W * alpha)),
            Metrics.Stroke.Hairline * scale);
    }

    public static float Title(ImDrawListPtr drawList, Rect panel, string title, float top, float scale)
    {
        var size = Typography.Measure(title, TitleStyle);
        Typography.Draw(drawList, new Vector2(panel.Center.X - size.X * 0.5f, top), title, PlacardTheme.TitleInk,
            TitleStyle);
        return top + size.Y + TitleGap * scale;
    }

    public static float TitleHeight(float scale) =>
        Typography.LineHeight(TitleStyle) + TitleGap * scale;

    public static void Surface(ImDrawListPtr drawList, Rect bounds, bool selected, bool hovered, bool strong,
        float scale)
    {
        var radius = Radius(scale);
        if (selected)
        {
            var fill = strong ? PlacardTheme.SelectionStrongFill : PlacardTheme.SelectionFill;
            Squircle.Fill(drawList, bounds.Min, bounds.Max, radius, ImGui.GetColorU32(fill));
            if (!strong)
            {
                Squircle.Stroke(drawList, bounds.Min, bounds.Max, radius,
                    ImGui.GetColorU32(PlacardTheme.FocusRing), Metrics.Stroke.Hairline * scale);
            }

            return;
        }

        if (hovered)
        {
            Squircle.Fill(drawList, bounds.Min, bounds.Max, radius,
                ImGui.GetColorU32(PlacardTheme.SelectionHover));
        }
    }

    public static void Marker(ImDrawListPtr drawList, Vector2 center, float scale, float alpha = 1f)
    {
        drawList.AddCircleFilled(center, MarkerRadius * scale,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Accent, alpha)), 14);
    }

    public static Vector4 Ink(bool selected, bool hovered, bool strong)
    {
        if (selected)
        {
            return strong ? PlacardTheme.AccentInk : PlacardTheme.SelectionInk;
        }

        return hovered ? PlacardTheme.TitleInk : PlacardTheme.BodyInk;
    }
}
