using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Animation;
using Placard.Core.Localization;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class LoadingPulse
{
    private const double SweepPeriodMs = 1100.0;
    private const double CorePulsePeriodMs = 1600.0;
    private const float SweepArcRadians = 1.6f;
    private const int SweepSegments = 24;
    private const float CaptionGap = 20f;

    public static string SafeLabel() => Loc.T(L.Common.Loading);

    public static void Draw(Vector2 center, float radius, Vector4 accent, Vector4 textColor, string? label,
        float alpha = 1f)
    {
        Spinner(center, radius, accent, alpha);
        if (label is null)
        {
            return;
        }

        var drawList = ImGui.GetWindowDrawList();
        var caretY = center.Y + radius + CaptionGap * UiScale.Current;
        var size = Typography.Measure(label, TextStyles.Footnote);
        Typography.Draw(drawList, new Vector2(center.X - size.X * 0.5f, caretY), label,
            Palette.WithAlpha(textColor, textColor.W * alpha), TextStyles.Footnote);
    }

    public static void Spinner(Vector2 center, float radius, Vector4 accent, float alpha = 1f)
    {
        if (alpha <= 0f)
        {
            return;
        }

        var drawList = ImGui.GetWindowDrawList();
        var thickness = MathF.Max(2f * UiScale.Current, radius * 0.10f);
        drawList.AddCircleFilled(center, radius * 1.9f,
            ImGui.GetColorU32(Palette.WithAlpha(accent, 0.06f * alpha)), 48);
        drawList.AddCircleFilled(center, radius * 1.25f,
            ImGui.GetColorU32(Palette.WithAlpha(accent, 0.10f * alpha)), 48);
        drawList.AddCircle(center, radius, ImGui.GetColorU32(Palette.WithAlpha(accent, 0.18f * alpha)), 72,
            thickness);

        var start = Pulse.Phase(SweepPeriodMs) * MathF.Tau;
        drawList.PathClear();
        drawList.PathArcTo(center, radius, start, start + SweepArcRadians, SweepSegments);
        drawList.PathStroke(ImGui.GetColorU32(Palette.WithAlpha(accent, alpha)), ImDrawFlags.None, thickness);

        var core = Palette.Mix(accent, Vector4.One, 0.7f);
        var pulse = 0.9f + 0.1f * Pulse.Wave(CorePulsePeriodMs);
        drawList.AddCircleFilled(center, radius * 0.26f * pulse,
            ImGui.GetColorU32(Palette.WithAlpha(core, alpha)), 32);
    }
}
