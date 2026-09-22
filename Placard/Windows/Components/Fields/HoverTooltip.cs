using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Animation;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal enum HoverLabelSide
{
    Above,
    Below,
}

internal static class HoverTooltip
{
    private const float SmoothTime = 0.12f;
    private const float RevealThreshold = 0.02f;
    private const float PaddingX = 10f;
    private const float PaddingY = 6f;
    private const float Offset = 8f;
    private const float MaxWidth = 260f;

    private static readonly Dictionary<string, Spring> Fades = new(StringComparer.Ordinal);

    public static void Show(string id, Rect anchor, string text, HoverLabelSide side = HoverLabelSide.Above)
    {
        if (text.Length == 0)
        {
            return;
        }

        var hovered = UiInteract.HoverWindowOnly(anchor.Min, anchor.Max);
        var progress = Animate(id, hovered ? 1f : 0f);
        if (progress <= RevealThreshold)
        {
            return;
        }

        Draw(anchor, text, side, progress);
    }

    public static void Show(Rect anchor, string text, HoverLabelSide side = HoverLabelSide.Above)
    {
        if (text.Length == 0 || !UiInteract.HoverWindowOnly(anchor.Min, anchor.Max))
        {
            return;
        }

        Draw(anchor, text, side, 1f);
    }

    private static void Draw(Rect anchor, string text, HoverLabelSide side, float progress)
    {
        var scale = UiScale.Current;
        var drawList = ImGui.GetForegroundDrawList();
        var maxWidth = MaxWidth * scale;
        var block = Typography.MeasureWrappedBlock(text, TextStyles.Footnote, maxWidth);
        var padding = new Vector2(PaddingX * scale, PaddingY * scale);
        var size = block + padding * 2f;
        var centerX = anchor.Center.X;
        var top = side == HoverLabelSide.Above
            ? anchor.Min.Y - Offset * scale - size.Y
            : anchor.Max.Y + Offset * scale;
        var min = new Vector2(centerX - size.X * 0.5f, top);
        var max = min + size;
        var alpha = Math.Clamp(progress, 0f, 1f);
        Squircle.Fill(drawList, min, max, Metrics.Radius.Sm * scale,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.SurfaceMuted, 0.96f * alpha)));
        Squircle.Stroke(drawList, min, max, Metrics.Radius.Sm * scale,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.CardStroke, PlacardTheme.CardStroke.W * alpha)), Metrics.Stroke.Hairline * scale);
        Typography.DrawWrappedLeft(drawList, min + padding, text,
            Palette.WithAlpha(PlacardTheme.BodyInk, alpha), TextStyles.Footnote, block.X);
    }

    private static float Animate(string id, float target)
    {
        if (!Fades.TryGetValue(id, out var spring))
        {
            spring = new Spring(target);
        }

        var deltaSeconds = MathF.Min(ImGui.GetIO().DeltaTime, 0.1f);
        var position = spring.Step(target, SmoothTime, deltaSeconds);
        Fades[id] = spring;
        return Math.Clamp(position, 0f, 1f);
    }
}
