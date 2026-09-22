using Placard.Core;
using Placard.Core.Housing;
using Placard.Core.Localization;
using Placard.Core.Theme;
using Placard.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace Placard.Windows.Housing;

internal sealed partial class HousingApp
{
    private const float WorldRowHeight = 44f;
    private const float WorldSearchHeight = 44f;
    private const float WorldScrollLead = 96f;

    private readonly List<HousingWorld> worldMatches = new();

    private bool worldScrollPending;
    private string stickyDataCenter = string.Empty;
    private float stickyNextHeaderTop;

    private void OpenWorldPicker()
    {
        worldSearch = string.Empty;
        worldScrollPending = true;
        Push(HousingRoute.WorldPicker);
    }

    private void DrawWorldPickerRoute(Rect area)
    {
        var scale = UiScale.Current;
        var body = DrawSubHeader(area, "housing.header.worlds", Loc.T(L.Housing.SelectWorldTitle));
        var pad = 16f * scale;
        var searchBar = new Rect(new Vector2(body.Min.X + pad, body.Min.Y + 4f * scale),
            new Vector2(body.Max.X - pad, body.Min.Y + 4f * scale + WorldSearchHeight * scale));
        SearchField.Draw(searchBar, "##housingWorldSearch", Loc.T(L.Housing.SearchWorlds), ref worldSearch, 40);
        var listBody = new Rect(new Vector2(body.Min.X, searchBar.Max.Y), body.Max);
        using (AppSurface.Begin(listBody))
        {
            var worlds = housing.Worlds;
            if (worlds.Count == 0)
            {
                var label = housing.WorldsLoading ? LoadingPulse.SafeLabel() : Loc.T(L.Housing.Offline);
                Typography.Draw(ImGui.GetCursorScreenPos() + new Vector2(2f * scale, 20f * scale), label, PlacardTheme.MutedInk,
                    TextStyles.Subheadline);
                ImGui.Dummy(new Vector2(ScrollLayout.StableContentWidth(), 60f * scale));
                return;
            }

            var query = worldSearch.Trim();
            if (query.Length > 0)
            {
                worldScrollPending = false;
                DrawWorldSearchResults(query, scale);
                return;
            }

            DrawWorldGroups(scale);
        }
    }

    private void DrawWorldSearchResults(string query, float scale)
    {
        worldMatches.Clear();
        var worlds = housing.Worlds;
        for (var index = 0; index < worlds.Count; index++)
        {
            if (worlds[index].Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                worlds[index].DataCenterName.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                worldMatches.Add(worlds[index]);
            }
        }

        if (worldMatches.Count == 0)
        {
            Typography.Draw(ImGui.GetCursorScreenPos() + new Vector2(2f * scale, 20f * scale),
                Loc.T(L.Housing.NoWorldMatches), PlacardTheme.MutedInk, TextStyles.Subheadline);
            ImGui.Dummy(new Vector2(ScrollLayout.StableContentWidth(), 60f * scale));
            return;
        }

        worldMatches.Sort(static (first, second) =>
            string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase));
        ImGui.Dummy(new Vector2(0f, 8f * scale));
        var card = GroupCard.Begin(worldMatches.Count, WorldRowHeight);
        for (var index = 0; index < worldMatches.Count; index++)
        {
            var world = worldMatches[index];
            if (DrawWorldRow(card.NextRow(), world.Name, DetailFor(world), IsCurrentWorld(world.Id), scale))
            {
                PickWorld(world.Id);
            }
        }

        card.End();
        ImGui.Dummy(new Vector2(0f, 16f * scale));
    }

    private void DrawWorldGroups(float scale)
    {
        var stickyLine = ImGui.GetWindowPos().Y;
        var headerHeight = DataCenterHeaderHeight(scale);
        stickyDataCenter = string.Empty;
        stickyNextHeaderTop = float.MaxValue;

        var regions = HousingRegions.Order;
        for (var regionIndex = 0; regionIndex < regions.Count; regionIndex++)
        {
            var region = regions[regionIndex];
            var dataCenters = CollectDataCenters(region);
            for (var dcIndex = 0; dcIndex < dataCenters.Count; dcIndex++)
            {
                var dataCenter = dataCenters[dcIndex];
                worldMatches.Clear();
                var worlds = housing.Worlds;
                for (var index = 0; index < worlds.Count; index++)
                {
                    if (string.Equals(worlds[index].DataCenterName, dataCenter, StringComparison.Ordinal))
                    {
                        worldMatches.Add(worlds[index]);
                    }
                }

                if (worldMatches.Count == 0)
                {
                    continue;
                }

                worldMatches.Sort(static (first, second) =>
                    string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase));
                var label = string.Concat(region, " · ", dataCenter);
                var headerTop = ImGui.GetCursorScreenPos().Y;
                SettingsSection.Header(label);
                var card = GroupCard.Begin(worldMatches.Count, WorldRowHeight);
                for (var index = 0; index < worldMatches.Count; index++)
                {
                    var world = worldMatches[index];
                    var detail = world.Id == housing.HomeWorldId ? Loc.T(L.Housing.HomeWorld) : string.Empty;
                    var current = IsCurrentWorld(world.Id);
                    var row = card.NextRow();
                    if (current && worldScrollPending)
                    {
                        ScrollToWorldRow(row.Min.Y, headerTop, scale);
                    }

                    if (DrawWorldRow(row, world.Name, detail, current, scale))
                    {
                        PickWorld(world.Id);
                    }
                }

                card.End();
                var groupBottom = ImGui.GetCursorScreenPos().Y;
                if (headerTop <= stickyLine && groupBottom > stickyLine)
                {
                    stickyDataCenter = label;
                }
                else if (headerTop > stickyLine && stickyNextHeaderTop > headerTop)
                {
                    stickyNextHeaderTop = headerTop;
                }
            }
        }

        worldScrollPending = false;
        ImGui.Dummy(new Vector2(0f, 20f * scale));
        DrawStickyDataCenter(stickyLine, headerHeight, scale);
    }

    private void ScrollToWorldRow(float rowTop, float headerTop, float scale)
    {
        var windowTop = ImGui.GetWindowPos().Y;
        var scrollY = ImGui.GetScrollY();
        var target = MathF.Max(headerTop, rowTop - WorldScrollLead * scale);
        ImGui.SetScrollY(MathF.Max(0f, scrollY + target - windowTop));
    }

    private static float DataCenterHeaderHeight(float scale) =>
        (Metrics.Space.Sm + Metrics.Space.Xs) * scale + Typography.LineHeight(TextStyles.FootnoteEmphasized);

    private void DrawStickyDataCenter(float stickyLine, float headerHeight, float scale)
    {
        if (stickyDataCenter.Length == 0)
        {
            return;
        }

        var top = stickyLine;
        if (stickyNextHeaderTop - stickyLine < headerHeight)
        {
            top = stickyNextHeaderTop - headerHeight;
        }

        var drawList = ImGui.GetWindowDrawList();
        var left = ImGui.GetWindowPos().X;
        var right = left + ImGui.GetWindowSize().X;
        drawList.AddRectFilled(new Vector2(left, top), new Vector2(right, top + headerHeight),
            ImGui.GetColorU32(PlacardTheme.Surface));
        drawList.AddLine(new Vector2(left, top + headerHeight), new Vector2(right, top + headerHeight),
            ImGui.GetColorU32(PlacardTheme.Separator), Metrics.Stroke.Hairline);
        Typography.Draw(drawList,
            new Vector2(left + (AppSurface.SidePadding + Metrics.Space.Lg) * scale, top + Metrics.Space.Sm * scale),
            stickyDataCenter.ToUpperInvariant(), PlacardTheme.HeaderInk, TextStyles.FootnoteEmphasized);
    }

    private readonly List<string> dataCenterBuffer = new();

    private List<string> CollectDataCenters(string region)
    {
        dataCenterBuffer.Clear();
        var worlds = housing.Worlds;
        for (var index = 0; index < worlds.Count; index++)
        {
            var world = worlds[index];
            if (!string.Equals(world.RegionName, region, StringComparison.Ordinal) ||
                dataCenterBuffer.Contains(world.DataCenterName))
            {
                continue;
            }

            dataCenterBuffer.Add(world.DataCenterName);
        }

        dataCenterBuffer.Sort(StringComparer.OrdinalIgnoreCase);
        return dataCenterBuffer;
    }

    private bool IsCurrentWorld(uint worldId) => housing.WorldId == worldId;

    private string DetailFor(HousingWorld world) =>
        world.Id == housing.HomeWorldId
            ? string.Concat(world.DataCenterName, " · ", Loc.T(L.Housing.HomeWorld))
            : world.DataCenterName;

    private void PickWorld(uint worldId)
    {
        housing.SelectWorld(worldId);
        ResetMapView();
        sheetOpen = false;
        selectedPlot = default;
        InvalidateCache();
        Pop();
    }

    private bool DrawWorldRow(Rect row, string name, string detail, bool selected, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var hovered = UiInteract.Hover(row.Min, row.Max);
        var bounds = new Rect(new Vector2(row.Min.X - Metrics.Space.Sm * scale, row.Min.Y + 2f * scale),
            new Vector2(row.Max.X + Metrics.Space.Sm * scale, row.Max.Y - 2f * scale));
        Selection.Surface(drawList, bounds, selected, hovered, false, scale);
        if (hovered)
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }

        var markerInset = Selection.MarkerInset * scale;
        if (selected)
        {
            Selection.Marker(drawList, new Vector2(bounds.Min.X + markerInset, row.Center.Y), scale);
        }

        var textLeft = row.Min.X + markerInset + Selection.MarkerRadius * scale;
        var detailStyle = TextStyles.Footnote;
        var detailSize = detail.Length > 0 ? Typography.Measure(detail, detailStyle) : Vector2.Zero;
        var nameMax = MathF.Max(1f, row.Max.X - textLeft - detailSize.X - Metrics.Space.Md * scale);
        var nameStyle = selected ? TextStyles.BodyEmphasized : TextStyles.Body;
        Typography.Draw(drawList, new Vector2(textLeft, row.Center.Y - Typography.LineHeight(nameStyle) * 0.5f),
            Typography.FitText(name, nameMax, nameStyle), Selection.Ink(selected, hovered, false), nameStyle);
        if (detail.Length > 0)
        {
            Typography.Draw(drawList, new Vector2(row.Max.X - detailSize.X, row.Center.Y - detailSize.Y * 0.5f),
                detail, selected ? PlacardTheme.SelectionInk : PlacardTheme.MutedInk, detailStyle);
        }

        return UiInteract.Click(row.Min, row.Max, hovered);
    }
}
