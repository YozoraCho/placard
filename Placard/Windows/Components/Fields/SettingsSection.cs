using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal static class SettingsSection
{
    public static void Header(string title)
    {
        var scale = UiScale.Current;
        ImGui.Dummy(new Vector2(0f, Metrics.Space.Sm * scale));
        var origin = ImGui.GetCursorScreenPos();
        var left = origin.X + Metrics.Space.Lg * scale;
        var label = title.ToUpperInvariant();
        var size = Typography.Measure(label, TextStyles.FootnoteEmphasized);
        Typography.Draw(ImGui.GetWindowDrawList(), new Vector2(left, origin.Y), label, PlacardTheme.HeaderInk,
            TextStyles.FootnoteEmphasized);
        ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, size.Y));
        ImGui.Dummy(new Vector2(0f, Metrics.Space.Xs * scale));
    }

    public static void Hint(string text)
    {
        var scale = UiScale.Current;
        var padding = Metrics.Space.Lg * scale;
        var origin = ImGui.GetCursorScreenPos();
        var width = MathF.Max(1f, ImGui.GetContentRegionAvail().X - padding * 2f);
        var height = Typography.DrawWrappedLeft(ImGui.GetWindowDrawList(),
            new Vector2(origin.X + padding, origin.Y), text, PlacardTheme.MutedInk, TextStyles.Footnote, width);
        ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, height + Metrics.Space.Sm * scale));
    }
}
