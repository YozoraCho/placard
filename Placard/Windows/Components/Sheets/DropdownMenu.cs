using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal sealed class DropdownMenu
{
    private const float MinimumWidth = 180f;
    private const float EdgeGap = 8f;

    private string ownerId = string.Empty;
    private Rect anchor;
    private bool open;
    private int openedFrame = -1;

    public bool Open => open;

    public string Header { get; set; } = string.Empty;

    public bool IsOpenFor(string id) => open && string.Equals(ownerId, id, StringComparison.Ordinal);

    public void Toggle(string id, Rect anchorRect)
    {
        if (IsOpenFor(id))
        {
            Close();
            return;
        }

        ownerId = id;
        anchor = anchorRect;
        open = true;
        openedFrame = ImGui.GetFrameCount();
    }

    public void Close()
    {
        open = false;
        ownerId = string.Empty;
    }

    public int Draw(Rect screen, ReadOnlySpan<Item> items)
    {
        if (!open || items.Length == 0)
        {
            return -1;
        }

        var scale = UiScale.Current;
        var drawList = ImGui.GetForegroundDrawList();
        var pad = Selection.PanelPadding * scale;
        var hasTitle = Header.Length > 0;
        var width = MinimumWidth * scale;
        for (var index = 0; index < items.Length; index++)
        {
            var natural = Typography.Measure(items[index].Label, Selection.RowStyle).X
                + (Selection.RowInset * 2f + Selection.MarkerInset) * scale;
            width = MathF.Max(width, natural);
        }

        if (hasTitle)
        {
            width = MathF.Max(width, Typography.Measure(Header, Selection.TitleStyle).X + pad * 2f);
        }

        width += pad * 2f;
        var rowsHeight = items.Length * Selection.RowHeight * scale;
        var height = pad * 2f + rowsHeight + (hasTitle ? Selection.TitleHeight(scale) : 0f);
        var left = MathF.Min(anchor.Min.X, screen.Max.X - width - EdgeGap * scale);
        left = MathF.Max(left, screen.Min.X + EdgeGap * scale);
        var top = MathF.Min(anchor.Max.Y + EdgeGap * scale, screen.Max.Y - height - EdgeGap * scale);
        top = MathF.Max(top, screen.Min.Y + EdgeGap * scale);
        var min = new Vector2(left, top);
        var panel = new Rect(min, min + new Vector2(width, height));
        UiInteract.HoverOverlay(panel);
        Selection.Backdrop(drawList, panel, scale);

        var rowTop = panel.Min.Y + pad;
        if (hasTitle)
        {
            rowTop = Selection.Title(drawList, panel, Header, rowTop, scale);
        }

        var picked = -1;
        for (var index = 0; index < items.Length; index++)
        {
            var bounds = new Rect(new Vector2(panel.Min.X + pad, rowTop + index * Selection.RowHeight * scale),
                new Vector2(panel.Max.X - pad, rowTop + (index + 1) * Selection.RowHeight * scale));
            var item = items[index];
            var hovered = UiInteract.HoverWindowOnly(bounds.Min, bounds.Max, false);
            Selection.Surface(drawList, bounds, item.Selected, hovered, false, scale);
            if (hovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            var labelLeft = bounds.Min.X + (Selection.RowInset + Selection.MarkerInset) * scale;
            var labelWidth = MathF.Max(1f, bounds.Max.X - Selection.RowInset * scale - labelLeft);
            var display = Typography.FitText(item.Label, labelWidth, Selection.RowStyle);
            var labelSize = Typography.Measure(display, Selection.RowStyle);
            var ink = item.Danger ? PlacardTheme.Danger : Selection.Ink(item.Selected, hovered, false);
            Typography.Draw(drawList, new Vector2(labelLeft, bounds.Center.Y - labelSize.Y * 0.5f), display, ink,
                Selection.RowStyle);
            if (item.Selected)
            {
                Selection.Marker(drawList,
                    new Vector2(bounds.Min.X + Selection.RowInset * scale, bounds.Center.Y), scale);
            }

            if (UiInteract.ClickImmediate(bounds.Min, bounds.Max, hovered))
            {
                picked = index;
            }
        }

        if (picked >= 0)
        {
            Close();
            return picked;
        }

        if (ImGui.GetFrameCount() != openedFrame && UiInteract.ClickedOutside(panel.Min, panel.Max, false))
        {
            Close();
        }

        return -1;
    }

    internal readonly record struct Item(string Label, string Glyph = "", bool Danger = false,
        bool Selected = false);
}
