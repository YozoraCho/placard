using Dalamud.Bindings.ImGui;
using Placard.Core;
using Placard.Core.Theme;

namespace Placard.Windows.Components;

internal struct GroupCard
{
    public const float DefaultRowHeight = Metrics.Size.Row;

    private readonly float scale;
    private readonly float rowHeight;
    private readonly float left;
    private readonly float right;
    private readonly float startY;
    private readonly int rowCount;
    private int rowIndex;

    private GroupCard(float scale, float rowHeight, float left, float right, float startY, int rowCount)
    {
        this.scale = scale;
        this.rowHeight = rowHeight;
        this.left = left;
        this.right = right;
        this.startY = startY;
        this.rowCount = rowCount;
        rowIndex = 0;
    }

    public static GroupCard Begin(int rowCount, float rowHeight = DefaultRowHeight)
    {
        var scale = UiScale.Current;
        var origin = ImGui.GetCursorScreenPos();
        var right = origin.X + ImGui.GetContentRegionAvail().X;
        var cardMax = new Vector2(right, origin.Y + rowCount * rowHeight * scale);
        var drawList = ImGui.GetWindowDrawList();
        var radius = Metrics.Radius.Md * scale;
        Squircle.Fill(drawList, origin, cardMax, radius, ImGui.GetColorU32(PlacardTheme.GroupedCard));
        Squircle.Stroke(drawList, origin, cardMax, radius, ImGui.GetColorU32(PlacardTheme.CardStroke),
            Metrics.Stroke.Hairline * scale);
        return new GroupCard(scale, rowHeight, origin.X, right, origin.Y, rowCount);
    }

    public Rect NextRow(int rowSpan = 1)
    {
        var rowTop = startY + rowIndex * rowHeight * scale;
        if (rowIndex > 0)
        {
            var separatorX = left + Metrics.Space.Lg * scale;
            ImGui.GetWindowDrawList().AddLine(new Vector2(separatorX, rowTop), new Vector2(right, rowTop),
                ImGui.GetColorU32(PlacardTheme.Separator), Metrics.Stroke.Hairline);
        }

        rowIndex += rowSpan;
        var padding = Metrics.Space.Lg * scale;
        return new Rect(new Vector2(left + padding, rowTop),
            new Vector2(right - padding, rowTop + rowSpan * rowHeight * scale));
    }

    public void End()
    {
        ImGui.SetCursorScreenPos(new Vector2(left, startY));
        ImGui.Dummy(new Vector2(right - left, rowCount * rowHeight * scale));
    }
}
