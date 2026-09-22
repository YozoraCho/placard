using Placard.Core;
using Placard.Core.Animation;
using Placard.Core.Housing;
using Placard.Core.Localization;
using Placard.Core.Theme;
using Placard.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace Placard.Windows.Housing;

internal sealed partial class HousingApp
{
    private const float MinZoom = 0.85f;
    private const float MaxZoom = 4.2f;
    private const float WheelStep = 0.14f;
    private const float ZoomButtonStep = 1.35f;
    private const float DragSlop = 5f;

    private const float LabelZoom = 1.45f;

    private void DrawMapRoute(Rect area)
    {
        var scale = UiScale.Current;
        var toolbar = new Rect(area.Min, new Vector2(area.Max.X, area.Min.Y + ToolbarHeight * scale));
        DrawToolbar(toolbar, scale);
        var phaseBar = new Rect(new Vector2(area.Min.X, toolbar.Max.Y),
            new Vector2(area.Max.X, toolbar.Max.Y + PhaseBarHeight * scale));
        DrawPhaseBar(phaseBar, scale);
        var bannerBottom = DrawDataBanner(area, phaseBar.Max.Y, scale);
        var status = new Rect(new Vector2(area.Min.X, area.Max.Y - StatusBarHeight * scale), area.Max);
        var body = new Rect(new Vector2(area.Min.X, bannerBottom), new Vector2(area.Max.X, status.Min.Y));

        if (!housing.HasWorldSelected)
        {
            DrawNoWorldState(body, scale);
            DrawStatusBar(status, scale);
            return;
        }

        if (BrowseMode == HousingBrowseMode.List)
        {
            DrawListBody(body, scale);
            DrawStatusBar(status, scale);
            if (filterSpring.Value > 0.005f)
            {
                using (AppSurface.BeginOverlay(body))
                {
                    DrawFilterDrawer(body, scale);
                }
            }

            return;
        }

        var listWidth = ListPaneWidthFor(body, scale);
        var viewport = new Rect(body.Min, new Vector2(body.Max.X - listWidth, body.Max.Y));
        if (housing.GameMap is null)
        {
            DrawEmptyCard(viewport, FontAwesomeIcon.MapSigns, Loc.T(L.Housing.GameMapUnavailable),
                Loc.T(L.Housing.GameMapUnavailableHint), Loc.T(L.Housing.ViewAsList),
                () => SetBrowseMode(HousingBrowseMode.List), scale);
        }
        else
        {
            DrawMapViewport(viewport, scale);
        }

        if (listWidth > 0f && !CoversBody)
        {
            DrawSidePane(new Rect(new Vector2(viewport.Max.X, body.Min.Y), body.Max), scale);
        }
        else if (sheetOpen && selectedPlot.IsValid)
        {
            sheetOpen = false;
            Push(HousingRoute.Details, selectedPlot);
        }

        DrawStatusBar(status, scale);
        DrawFilterDrawer(viewport, scale);
        DrawWardPicker(viewport, viewport, scale);
    }

    private static float ListPaneWidthFor(Rect body, float scale)
    {
        if (body.Width < (ListPaneMinWidth + MapPaneMinWidth) * scale)
        {
            return 0f;
        }

        var ideal = MathF.Min(ListPaneWidth * scale, body.Width * ListPaneMaxFraction);
        return Math.Clamp(ideal, ListPaneMinWidth * scale, body.Width - MapPaneMinWidth * scale);
    }

    private void DrawToolbar(Rect bar, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var pad = Metrics.Space.Md * scale;
        var gap = Metrics.Space.Xs * scale;
        var height = HousingChrome.SelectorHeight(scale);
        var top = bar.Center.Y - height * 0.5f;
        var buttonRadius = 13f * scale;
        var buttonGap = 30f * scale;
        var closeCenter = new Vector2(bar.Max.X - pad - buttonRadius, bar.Center.Y);
        var settingsCenter = new Vector2(closeCenter.X - buttonGap, bar.Center.Y);
        var watchCenter = new Vector2(settingsCenter.X - buttonGap, bar.Center.Y);

        var markCenter = new Vector2(bar.Min.X + pad + 9f * scale, bar.Center.Y);
        HousingGlyphs.Estate(drawList, markCenter, 9f * scale, PlacardTheme.Accent, PlacardTheme.Surface);
        var selectorLeft = markCenter.X + 18f * scale;

        var mapLabel = Loc.T(L.Housing.Map);
        var listLabel = Loc.T(L.Housing.List);
        var segmentWidth = SegmentWidthFor(mapLabel, listLabel, scale);
        var segmentRight = watchCenter.X - buttonRadius - Metrics.Space.Md * scale;
        var segmentRect = new Rect(new Vector2(segmentRight - segmentWidth, top),
            new Vector2(segmentRight, top + height));

        var listMode = BrowseMode == HousingBrowseMode.List;
        var available = segmentRect.Min.X - Metrics.Space.Md * scale - selectorLeft - gap * 2f;
        available = MathF.Max(available, 120f * scale);
        var worldWidth = available * 0.42f;
        var districtWidth = available * 0.35f;
        var wardWidth = available - worldWidth - districtWidth;
        var x = selectorLeft;
        var worldRect = new Rect(new Vector2(x, top), new Vector2(x + worldWidth, top + height));
        x = worldRect.Max.X + gap;
        var districtRect = new Rect(new Vector2(x, top), new Vector2(x + districtWidth, top + height));
        x = districtRect.Max.X + gap;
        var wardRect = new Rect(new Vector2(x, top), new Vector2(x + wardWidth, top + height));

        var worldName = housing.WorldName;
        if (HousingChrome.Selector(worldRect, Loc.T(L.Housing.WorldLabel),
                worldName.Length > 0 ? worldName : Loc.T(L.Housing.ChooseWorld), false))
        {
            worldSearch = string.Empty;
            OpenWorldPicker();
        }

        if (!listMode)
        {
            if (HousingChrome.Selector(districtRect, Loc.T(L.Housing.DistrictLabel),
                    HousingDistricts.ShortDisplayName(housing.DistrictId), false))
            {
                OpenDistrictMenu(districtRect);
            }

            if (HousingChrome.Selector(wardRect, Loc.T(L.Housing.WardLabel), housing.Ward.ToString(Loc.Culture),
                    false))
            {
                ShowOverlay(wardPickerOpen ? HousingOverlay.None : HousingOverlay.WardPicker);
            }
        }

        if (HousingChrome.Segment(segmentRect, mapLabel, listLabel, listMode ? 1 : 0) == 1 != listMode)
        {
            SetBrowseMode(listMode ? HousingBrowseMode.Map : HousingBrowseMode.List);
        }

        if (HousingChrome.MapButton(watchCenter, buttonRadius, FontAwesomeIcon.Bookmark,
                Loc.T(L.Housing.Watchlist), false, false))
        {
            Push(HousingRoute.Watchlist);
        }

        if (HousingChrome.MapButton(settingsCenter, buttonRadius, FontAwesomeIcon.Cog,
                Loc.T(L.Housing.Settings), false, false))
        {
            Push(HousingRoute.Settings);
        }

        if (HousingChrome.CloseButton(closeCenter, buttonRadius, false))
        {
            closeRequested();
        }

        var watchCount = housing.Watch.Watched.Count;
        if (watchCount > 0)
        {
            var badgeCenter = new Vector2(watchCenter.X + buttonRadius * 0.72f, watchCenter.Y - buttonRadius * 0.72f);
            drawList.AddCircleFilled(badgeCenter, 5f * scale, ImGui.GetColorU32(PlacardTheme.Brass), 16);
        }

        drawList.AddLine(new Vector2(bar.Min.X, bar.Max.Y), new Vector2(bar.Max.X, bar.Max.Y),
            ImGui.GetColorU32(PlacardTheme.Separator), Metrics.Stroke.Hairline);

        var selectorsRight = listMode ? worldRect.Max.X : wardRect.Max.X;
        var overControls = ImGui.IsMouseHoveringRect(new Vector2(worldRect.Min.X, bar.Min.Y),
                               new Vector2(selectorsRight, bar.Max.Y), false) ||
                           ImGui.IsMouseHoveringRect(new Vector2(segmentRect.Min.X, bar.Min.Y),
                               new Vector2(closeCenter.X + buttonRadius, bar.Max.Y), false);
        DragHandleHovered = !overControls && UiInteract.HoverWindowOnly(bar.Min, bar.Max, false);
    }

    private static float SegmentWidthFor(string first, string second, float scale)
    {
        var widest = MathF.Max(Typography.Measure(first, TextStyles.SubheadlineEmphasized).X,
            Typography.Measure(second, TextStyles.SubheadlineEmphasized).X);
        return MathF.Max(SegmentMinWidth * scale, (widest + Metrics.Space.Lg * scale) * 2f);
    }

    private void DrawSidePane(Rect pane, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddLine(new Vector2(pane.Min.X, pane.Min.Y), new Vector2(pane.Min.X, pane.Max.Y),
            ImGui.GetColorU32(PlacardTheme.Separator), Metrics.Stroke.Hairline);
        if (sheetOpen && FindPlot(selectedPlot) is { } plot)
        {
            DrawPlotPane(pane, plot, scale);
            return;
        }

        sheetOpen = false;
        DrawPlotListPane(pane, scale);
    }

    private void DrawPlotPane(Rect pane, HousingPlot plot, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var pad = Metrics.Space.Md * scale;
        var header = new Rect(pane.Min, new Vector2(pane.Max.X, pane.Min.Y + PaneHeaderHeight * scale));
        var backCenter = new Vector2(header.Min.X + pad + 6f * scale, header.Center.Y);
        var backHit = new Vector2(13f * scale, 13f * scale);
        var backHovered = UiInteract.Hover(backCenter - backHit, backCenter + backHit);
        if (BackButton.Draw(backCenter, 11f * scale, PlacardTheme.Accent, backHovered, scale))
        {
            sheetOpen = false;
            reminderPickerOpen = false;
            return;
        }

        var titleLeft = backCenter.X + 16f * scale;
        var titleStyle = TextStyles.SubheadlineEmphasized;
        Typography.Draw(drawList, new Vector2(titleLeft, header.Center.Y - Typography.LineHeight(titleStyle) * 0.5f),
            Typography.FitText(HousingFormat.PlotTitle(plot), header.Max.X - pad - titleLeft, titleStyle),
            PlacardTheme.TitleInk, titleStyle);
        drawList.AddLine(new Vector2(header.Min.X, header.Max.Y), new Vector2(header.Max.X, header.Max.Y),
            ImGui.GetColorU32(PlacardTheme.Separator), Metrics.Stroke.Hairline);

        var content = new Rect(new Vector2(pane.Min.X + pad, header.Max.Y + pad),
            new Vector2(pane.Max.X - pad, pane.Max.Y - pad));
        DrawPlotPanelContent(content, plot, scale);
    }

    private void DrawPlotListPane(Rect pane, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var plots = FilteredDistrictPlots();
        var header = new Rect(pane.Min, new Vector2(pane.Max.X, pane.Min.Y + PaneHeaderHeight * scale));
        var pad = Metrics.Space.Md * scale;
        var sortLabel = Loc.T(SortLabels[Math.Clamp(configuration.HousingListSort, 0, SortLabels.Length - 1)]);
        var sortWidth = HousingChrome.MeasurePill(sortLabel, 22f * scale);
        var sortRect = new Rect(new Vector2(header.Max.X - pad - sortWidth, header.Center.Y - 11f * scale),
            new Vector2(header.Max.X - pad, header.Center.Y + 11f * scale));
        var countText = Loc.T(L.Housing.MatchingPlots, plots.Count);
        Typography.Draw(drawList,
            new Vector2(header.Min.X + pad, header.Center.Y - Typography.LineHeight(TextStyles.Caption1) * 0.5f),
            Typography.FitText(countText, sortRect.Min.X - header.Min.X - pad * 2f, TextStyles.Caption1),
            PlacardTheme.HeaderInk, TextStyles.Caption1);
        if (HousingChrome.PillButton(sortRect, sortLabel, false, false))
        {
            OpenSortMenu(sortRect);
        }

        var body = new Rect(new Vector2(pane.Min.X, header.Max.Y), pane.Max);
        if (plots.Count == 0)
        {
            var empty = Loc.T(housing.Filters.HasNarrowingFilters ? L.Housing.NoFilterMatches : L.Housing.NoScans);
            Typography.DrawWrappedCentered(drawList, body.Center, empty, PlacardTheme.MutedInk,
                TextStyles.Footnote, body.Width - pad * 2f);
            return;
        }

        using (AppSurface.Begin(body, Metrics.Space.Sm))
        {
            var now = DateTime.UtcNow;
            var width = ScrollLayout.StableContentWidth();
            var origin = ImGui.GetCursorScreenPos();
            for (var index = 0; index < plots.Count; index++)
            {
                var rowTop = origin.Y + index * ListRowHeight * scale;
                var row = new Rect(new Vector2(origin.X, rowTop),
                    new Vector2(origin.X + width, rowTop + ListRowHeight * scale));
                if (DrawListRow(row, plots[index], now, scale, false))
                {
                    OpenFromList(plots[index]);
                }
            }

            ImGui.Dummy(new Vector2(width, plots.Count * ListRowHeight * scale));
        }
    }

    private void DrawStatusBar(Rect status, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddLine(new Vector2(status.Min.X, status.Min.Y), new Vector2(status.Max.X, status.Min.Y),
            ImGui.GetColorU32(PlacardTheme.Separator), Metrics.Stroke.Hairline);
        var pad = Metrics.Space.Md * scale;
        var centerY = status.Center.Y + PlacardWindow.FrameInset * scale * 0.5f;
        var refreshRadius = 12f * scale;
        var refreshCenter = new Vector2(status.Max.X - pad - refreshRadius, centerY);
        var filterLabel = housing.Filters.ActiveCount > 0
            ? Loc.T(L.Housing.FiltersCount, housing.Filters.ActiveCount)
            : Loc.T(L.Housing.Filters);
        var filterHeight = 23f * scale;
        var filterWidth = HousingChrome.MeasurePill(filterLabel, filterHeight);
        var filterRight = refreshCenter.X - refreshRadius - Metrics.Space.Sm * scale;
        var filterRect = new Rect(new Vector2(filterRight - filterWidth, centerY - filterHeight * 0.5f),
            new Vector2(filterRight, centerY + filterHeight * 0.5f));

        var freshness = FooterFreshness();
        var chipLabel = HousingFormat.FreshnessLabel(freshness);
        var chipWidth = HousingChrome.MeasureChip(chipLabel);
        HousingChrome.Chip(drawList, new Vector2(status.Min.X + pad, centerY - HousingChrome.ChipHeight * scale * 0.5f),
            chipLabel, HousingChrome.FreshnessHue(freshness, PlacardTheme.Accent), false);
        var textLeft = status.Min.X + pad + chipWidth + Metrics.Space.Xs * scale;
        var textMax = MathF.Max(1f, filterRect.Min.X - Metrics.Space.Sm * scale - textLeft);
        Typography.Draw(drawList,
            new Vector2(textLeft, centerY - Typography.LineHeight(TextStyles.Footnote) * 0.5f),
            Typography.FitText(FooterStatusText(), textMax, TextStyles.Footnote), PlacardTheme.MutedInk,
            TextStyles.Footnote);

        if (HousingChrome.PillButton(filterRect, filterLabel, housing.Filters.ActiveCount > 0, false))
        {
            ShowOverlay(filtersOpen ? HousingOverlay.None : HousingOverlay.Filters);
        }

        var busy = housing.IsRefreshing || refreshFeedback;
        if (HousingChrome.RefreshButton(refreshCenter, refreshRadius, busy, Loc.T(L.Housing.Refresh), false))
        {
            RequestRefresh();
        }
    }

    private static void DrawAppEmblem(ImDrawListPtr drawList, Vector2 center, float size)
    {
        HousingGlyphs.Estate(drawList, center, size * 0.31f, PlacardTheme.Accent, PlacardTheme.Backdrop);
    }

    private void DrawPhaseBar(Rect bar, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var pad = 14f * scale;
        var plots = VisiblePlots();
        var now = DateTime.UtcNow;
        var phase = HousingLotteryPhase.Unknown;
        DateTime? soonest = null;
        for (var index = 0; index < plots.Count; index++)
        {
            var plot = plots[index];
            if (plot.PhaseEndsUtc is not { } ends || plot.Phase == HousingLotteryPhase.Unknown)
            {
                continue;
            }

            if (soonest is null || ends < soonest)
            {
                soonest = ends;
                phase = plot.Phase;
            }
        }

        if (soonest is not null && HousingFormat.HasExpired(soonest, now))
        {
            phase = HousingLotteryPhase.Expired;
            housing.RefreshAfterExpiry();
        }

        var label = Loc.Culture.TextInfo.ToUpper(HousingFormat.PhaseLabel(phase));
        var labelStyle = TextStyles.Caption1;
        Typography.Draw(drawList, new Vector2(bar.Min.X + pad, bar.Center.Y - Typography.LineHeight(labelStyle) * 0.5f),
            label, PlacardTheme.HeaderInk, labelStyle);
        var timerText = phase switch
        {
            HousingLotteryPhase.Expired => Loc.T(L.Housing.PhaseExpired),
            _ when soonest is null => Loc.T(L.Housing.TimeUnknown),
            _ => Loc.T(L.Housing.Remaining, HousingFormat.Countdown(HousingFormat.Remaining(soonest, now)
                ?? TimeSpan.Zero)),
        };
        var timerStyle = TextStyles.SubheadlineEmphasized;
        var timerSize = Typography.Measure(timerText, timerStyle);
        Typography.Draw(drawList, new Vector2(bar.Max.X - pad - timerSize.X, bar.Center.Y - timerSize.Y * 0.5f),
            timerText, phase == HousingLotteryPhase.Entry ? PlacardTheme.TitleInk : PlacardTheme.BodyInk, timerStyle);
    }

    private float DrawDataBanner(Rect area, float top, float scale)
    {
        if (housing.ActiveSource != HousingProviderKind.Cache)
        {
            return top;
        }

        var drawList = ImGui.GetWindowDrawList();
        var pad = 14f * scale;
        var text = Loc.T(L.Housing.CachedBanner,
            HousingFormat.AgeRelative(housing.Snapshot?.FetchedUtc ?? default, DateTime.UtcNow));
        var style = TextStyles.Caption1;
        var textWidth = area.Width - pad * 2f - 20f * scale;
        var textHeight = Typography.MeasureWrappedBlock(text, style, textWidth).Y;
        var height = textHeight + 12f * scale;
        var min = new Vector2(area.Min.X + pad, top + 2f * scale);
        var max = new Vector2(area.Max.X - pad, min.Y + height);
        var hue = PlacardTheme.Parchment;
        var rounding = Metrics.Radius.Sm * scale;
        Squircle.Fill(drawList, min, max, rounding, ImGui.GetColorU32(Palette.WithAlpha(hue, 0.16f)));
        Squircle.Stroke(drawList, min, max, rounding, ImGui.GetColorU32(Palette.WithAlpha(hue, 0.42f)),
            Metrics.Stroke.Hairline);
        var ink = Palette.Mix(hue, new Vector4(1f, 1f, 1f, 1f), 0.45f);
        Typography.DrawWrappedCentered(drawList, new Vector2((min.X + max.X) * 0.5f, (min.Y + max.Y) * 0.5f), text,
            ink, style, textWidth);
        return max.Y + 2f * scale;
    }

    private void DrawMapViewport(Rect viewport, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var plan = CurrentPlan();
        var plots = VisiblePlots();
        var controlsBlocked = ReserveControls(viewport, scale, out var zoomIn, out var zoomOut, out var recenter,
            out var legendButton, out var buttonRadius);
        var hintRect = FirstUseHintRect(viewport, plots, scale);
        var pointerOverHint = hintRect is { } hint && ImGui.IsMouseHoveringRect(hint.Min, hint.Max, false);
        var divisionRect = DivisionSwitchRect(viewport, plan, scale);
        var pointerOverDivision = divisionRect is { } division &&
                                  ImGui.IsMouseHoveringRect(division.Min, division.Max, false);
        var gestureBlocked = controlsBlocked || pointerOverHint || pointerOverDivision || filtersOpen ||
                             wardPickerOpen || reminderPickerOpen || menu.Open;
        var mapSize = MapSize(viewport);
        HandleGesture(viewport, mapSize, gestureBlocked, plan, plots, scale);
        var origin = MapOrigin(viewport, mapSize);
        drawList.PushClipRect(viewport.Min, viewport.Max, true);
        DrawPlan(drawList, plan, origin, mapSize, scale);
        DrawMarkers(drawList, plan, plots, origin, mapSize, viewport, scale);
        drawList.PopClipRect();
        DrawViewportEdges(drawList, viewport, scale);
        DrawStateOverlay(viewport, plots, scale);
        DrawMapControls(zoomIn, zoomOut, recenter, legendButton, buttonRadius);
        DrawDivisionSwitch(viewport, plan, scale);
        if (legendOpen)
        {
            DrawLegend(viewport, scale);
        }

        DrawFirstUseHint(viewport, plots, scale);
    }

    private bool ReserveControls(Rect viewport, float scale, out Vector2 zoomIn, out Vector2 zoomOut,
        out Vector2 recenter, out Vector2 legendButton, out float radius)
    {
        radius = 15f * scale;
        var right = viewport.Max.X - 14f * scale - radius;
        var spacing = radius * 2.35f;
        var firstY = viewport.Min.Y + 16f * scale + radius;
        zoomIn = new Vector2(right, firstY);
        zoomOut = new Vector2(right, firstY + spacing);
        recenter = new Vector2(right, firstY + spacing * 2f);
        legendButton = new Vector2(viewport.Min.X + 14f * scale + radius, viewport.Min.Y + 16f * scale + radius);
        var mouse = ImGui.GetMousePos();
        return Near(mouse, zoomIn, radius) || Near(mouse, zoomOut, radius) || Near(mouse, recenter, radius) ||
               Near(mouse, legendButton, radius);
    }

    private static bool Near(Vector2 point, Vector2 center, float radius)
    {
        var offset = point - center;
        return offset.LengthSquared() <= radius * radius * 1.44f;
    }

    private void DrawMapControls(Vector2 zoomIn, Vector2 zoomOut, Vector2 recenter, Vector2 legendButton,
        float radius)
    {
        if (HousingChrome.MapButton(zoomIn, radius, FontAwesomeIcon.Plus, Loc.T(L.Housing.ZoomIn), false,
                false))
        {
            ZoomAround(zoomIn, zoomTarget * ZoomButtonStep);
        }

        if (HousingChrome.MapButton(zoomOut, radius, FontAwesomeIcon.Minus, Loc.T(L.Housing.ZoomOut), false,
                false))
        {
            ZoomAround(zoomOut, zoomTarget / ZoomButtonStep);
        }

        var canRecenter = selectedPlot.IsValid;
        if (HousingChrome.MapButton(recenter, radius,
                canRecenter ? FontAwesomeIcon.Crosshairs : FontAwesomeIcon.Expand,
                canRecenter ? Loc.T(L.Housing.Recenter) : Loc.T(L.Housing.ResetMap), false, false))
        {
            if (canRecenter)
            {
                CenterOnSelected();
            }
            else
            {
                ResetMapView();
            }
        }

        if (HousingChrome.MapButton(legendButton, radius, FontAwesomeIcon.Question, Loc.T(L.Housing.Legend),
                legendOpen, false))
        {
            legendOpen = !legendOpen;
        }
    }

    private float MapSize(Rect viewport) =>
        MathF.Min(viewport.Width, viewport.Height) * 0.94f * zoomSpring.Value;

    private Vector2 MapOrigin(Rect viewport, float mapSize) =>
        viewport.Center - new Vector2(mapSize, mapSize) * 0.5f + new Vector2(panXSpring.Value, panYSpring.Value);

    private Vector2 ToScreen(Vector2 origin, float mapSize, Vector2 normalized) =>
        origin + normalized * mapSize;

    private void ResetMapView()
    {
        zoomTarget = 1f;
        panTarget = Vector2.Zero;
        zoomSpring.SnapTo(1f);
        panXSpring.SnapTo(0f);
        panYSpring.SnapTo(0f);
    }

    private void ZoomAround(Vector2 anchor, float requestedZoom)
    {
        var clamped = Math.Clamp(requestedZoom, MinZoom, MaxZoom);
        if (MathF.Abs(clamped - zoomTarget) < 0.0001f)
        {
            return;
        }

        var ratio = clamped / zoomTarget;
        var offset = anchor - LastViewportCenter;
        panTarget = (panTarget - offset) * ratio + offset;
        zoomTarget = clamped;
        ClampPan();
    }

    private Vector2 LastViewportCenter { get; set; }

    private void ClampPan()
    {
        var span = LastMapSpan;
        panTarget = new Vector2(Math.Clamp(panTarget.X, -span.X, span.X), Math.Clamp(panTarget.Y, -span.Y, span.Y));
    }

    private Vector2 LastMapSpan { get; set; }

    private void CenterOnSelected()
    {
        if (!selectedPlot.IsValid)
        {
            return;
        }

        var normalized = CurrentPlan().PositionOf(selectedPlot.Plot);
        if (zoomTarget < LabelZoom)
        {
            zoomTarget = LabelZoom;
        }

        var mapSize = MathF.Min(LastViewportSize.X, LastViewportSize.Y) * 0.94f * zoomTarget;
        var offsetFromCenter = (normalized - new Vector2(0.5f, 0.5f)) * mapSize;
        panTarget = -offsetFromCenter;
        ClampPan();
    }

    private Vector2 LastViewportSize { get; set; }

    private void HandleGesture(Rect viewport, float mapSize, bool blocked, in HousingPlan plan,
        List<HousingPlot> plots, float scale)
    {
        LastViewportCenter = viewport.Center;
        LastViewportSize = viewport.Size;
        LastMapSpan = new Vector2(MathF.Max(0f, mapSize * 0.5f), MathF.Max(0f, mapSize * 0.5f));
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(viewport.Min);
        ImGui.InvisibleButton("##housingMap", viewport.Size, ImGuiButtonFlags.MouseButtonLeft);
        var active = ImGui.IsItemActive();
        var hovered = ImGui.IsItemHovered();
        ImGui.SetCursorScreenPos(cursor);
        if (blocked)
        {
            dragging = false;
            return;
        }

        if (hovered)
        {
            var wheel = ImGui.GetIO().MouseWheel;
            if (wheel != 0f)
            {
                ZoomAround(ImGui.GetMousePos(), zoomTarget * (1f + wheel * WheelStep));
            }
        }

        if (active && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
        {
            dragging = true;
            dragTravel = 0f;
        }

        if (dragging && active)
        {
            var delta = ImGui.GetIO().MouseDelta;
            dragTravel += delta.Length();
            if (dragTravel > DragSlop * scale)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                panTarget += delta;
                panXSpring.SnapTo(panTarget.X);
                panYSpring.SnapTo(panTarget.Y);
                ClampPan();
            }

            return;
        }

        if (!dragging || !ImGui.IsMouseReleased(ImGuiMouseButton.Left))
        {
            return;
        }

        dragging = false;
        if (dragTravel > DragSlop * scale)
        {
            return;
        }

        var origin = MapOrigin(viewport, mapSize);
        HandleTap(ImGui.GetMousePos(), origin, mapSize, plan, plots, scale);
    }

    private void HandleTap(Vector2 point, Vector2 origin, float mapSize, in HousingPlan plan,
        List<HousingPlot> plots, float scale)
    {
        var hit = HousingMarkers.HitRadius * scale;
        var bestDistance = hit * hit;
        var found = false;
        var best = default(HousingPlotKey);
        for (var index = 0; index < plots.Count; index++)
        {
            var plot = plots[index];
            if (!plan.TryGetPoint(plot.Key.Plot, out var mapPoint))
            {
                continue;
            }

            var center = ToScreen(origin, mapSize, mapPoint);
            var distance = (center - point).LengthSquared();
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            best = plot.Key;
            found = true;
        }

        if (!found)
        {
            if (sheetOpen)
            {
                sheetOpen = false;
            }

            return;
        }

        SelectPlot(best);
    }

    private void SelectPlot(HousingPlotKey key)
    {
        if (selectedPlot == key && sheetOpen)
        {
            return;
        }

        selectedPlot = key;
        sheetOpen = true;
        reminderPickerOpen = false;
        reminderChoice = IndexOfLeadTime(configuration.HousingReminderMinutes);
    }

    private static int IndexOfLeadTime(int minutes)
    {
        var choices = HousingDefaults.ReminderChoices;
        for (var index = 0; index < choices.Length; index++)
        {
            if (choices[index] == minutes)
            {
                return index;
            }
        }

        return 2;
    }

    private HousingPlotKey? HoveredMarker(Vector2 origin, float mapSize, in HousingPlan plan,
        List<HousingPlot> plots, float scale, Rect viewport)
    {
        var mouse = ImGui.GetMousePos();
        if (!viewport.Contains(mouse))
        {
            return null;
        }

        var hit = HousingMarkers.HitRadius * scale;
        var bestDistance = hit * hit;
        HousingPlotKey? best = null;
        for (var index = 0; index < plots.Count; index++)
        {
            if (!plan.TryGetPoint(plots[index].Key.Plot, out var mapPoint))
            {
                continue;
            }

            var center = ToScreen(origin, mapSize, mapPoint);
            var distance = (center - mouse).LengthSquared();
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            best = plots[index].Key;
        }

        return best;
    }

    private HousingPlan CurrentPlan() => new(housing.GameMap, showSubdivision);

    private void DrawPlan(ImDrawListPtr drawList, in HousingPlan plan, Vector2 origin, float mapSize, float scale)
    {
        if (plan.Map is { } gameMap && DrawGameMapTexture(drawList, gameMap, origin, mapSize, scale))
        {
            return;
        }

        var min = origin;
        var max = origin + new Vector2(mapSize, mapSize);
        var rounding = 10f * scale;
        Squircle.Fill(drawList, min, max, rounding, ImGui.GetColorU32(PlacardTheme.MapPanel));
        Squircle.Stroke(drawList, min, max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Brass, 0.24f)), 1.2f * scale);
    }

    private bool DrawGameMapTexture(ImDrawListPtr drawList, HousingGameMap gameMap, Vector2 origin, float mapSize,
        float scale)
    {
        var texture = housing.GameMaps.Texture(gameMap);
        if (texture is null)
        {
            return false;
        }

        var min = origin;
        var max = origin + new Vector2(mapSize, mapSize);
        var rounding = 10f * scale;
        Squircle.Fill(drawList, min, max, rounding, ImGui.GetColorU32(PlacardTheme.MapPanel));
        drawList.AddImageRounded(texture.Handle, min, max, Vector2.Zero, Vector2.One,
            ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 0.94f)), rounding, ImDrawFlags.RoundCornersAll);
        drawList.AddRectFilled(min, max, ImGui.GetColorU32(PlacardTheme.MapWash), rounding);
        Squircle.Stroke(drawList, min, max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Brass, 0.34f)), 1.4f * scale);
        return true;
    }

    private void DrawMarkers(ImDrawListPtr drawList, in HousingPlan plan, List<HousingPlot> plots,
        Vector2 origin, float mapSize, Rect viewport, float scale)
    {
        if (housing.Filters.ShowAllPlots)
        {
            var tint = PlacardTheme.Parchment;
            for (var index = 0; index < plan.PlotCount; index++)
            {
                HousingMarkers.DrawBackground(drawList, ToScreen(origin, mapSize, plan.PlotAt(index)), scale, tint);
            }
        }

        var hovered = HoveredMarker(origin, mapSize, plan, plots, scale, viewport);
        var emphasis = Pulse.Wave(Pulse.Calm);
        var showLabels = zoomSpring.Value >= LabelZoom;
        var cull = HousingMarkers.HitRadius * 2f * scale;
        for (var index = 0; index < plots.Count; index++)
        {
            var plot = plots[index];
            if (!plan.TryGetPoint(plot.Key.Plot, out var mapPoint))
            {
                continue;
            }

            var center = ToScreen(origin, mapSize, mapPoint);
            if (center.X < viewport.Min.X - cull || center.X > viewport.Max.X + cull ||
                center.Y < viewport.Min.Y - cull || center.Y > viewport.Max.Y + cull)
            {
                continue;
            }

            var isSelected = selectedPlot == plot.Key;
            var style = new HousingMarkerStyle(plot.Size, plot.Phase, housing.Watch.IsWatched(plot.Key), isSelected,
                IsStale(plot), hovered == plot.Key);
            HousingMarkers.Draw(drawList, center, style, PlacardTheme.Accent, scale, isSelected ? emphasis : 0f);
            var wantsLabel = showLabels || isSelected || style.Watched;
            if (!wantsLabel)
            {
                continue;
            }

            var radius = HousingMarkers.Radius * scale * HousingMarkers.SizeScale(plot.Size);
            HousingMarkers.DrawLabel(drawList, center, radius, plot.Key.Plot.ToString(Loc.Culture), scale);
        }
    }

    private void DrawViewportEdges(ImDrawListPtr drawList, Rect viewport, float scale)
    {
        var fade = 18f * scale;
        var top = ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Backdrop, 0.85f));
        var clear = ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Backdrop, 0f));
        drawList.AddRectFilledMultiColor(viewport.Min, new Vector2(viewport.Max.X, viewport.Min.Y + fade), top, top,
            clear, clear);
    }

    private void DrawStateOverlay(Rect viewport, List<HousingPlot> plots, float scale)
    {
        if (plots.Count > 0)
        {
            return;
        }

        if (housing.Snapshot is null)
        {
            if (housing.IsRefreshing)
            {
                DrawLoading(viewport, Loc.T(L.Housing.LoadingFirst), scale);
                return;
            }

            DrawOfflineState(viewport, scale);
            return;
        }

        if (housing.Filters.HasNarrowingFilters && WardHasReportedPlots())
        {
            DrawEmptyCard(viewport, FontAwesomeIcon.Filter, Loc.T(L.Housing.NoFilterMatches), null,
                Loc.T(L.Housing.ClearFilters), () =>
                {
                    housing.Filters.Reset();
                    housing.PersistFilterDefaults();
                    InvalidateCache();
                }, scale);
            return;
        }

        if (housing.Snapshot is { Plots.Count: 0 })
        {
            DrawEmptyCard(viewport, FontAwesomeIcon.MapSigns, Loc.T(L.Housing.NoScans),
                Loc.T(L.Housing.NoScansHint), Loc.T(L.Housing.ChooseWard), () => wardPickerOpen = true, scale);
            return;
        }

        if (OtherDivisionHasPlots())
        {
            DrawEmptyCard(viewport, FontAwesomeIcon.Home, Loc.T(L.Housing.NoOpenings, housing.Ward), null,
                Loc.T(showSubdivision ? L.Housing.MainDivision : L.Housing.Subdivision), SwitchDivision, scale);
            return;
        }

        DrawEmptyCard(viewport, FontAwesomeIcon.Home, Loc.T(L.Housing.NoOpenings, housing.Ward), null,
            Loc.T(L.Housing.ChooseWard), () => wardPickerOpen = true, scale);
    }

    private bool OtherDivisionHasPlots()
    {
        if (housing.GameMap is not { HasSubdivision: true } || housing.Snapshot is not { } snapshot)
        {
            return false;
        }

        var ward = housing.Ward;
        var wanted = !showSubdivision;
        var plots = snapshot.Plots;
        for (var index = 0; index < plots.Count; index++)
        {
            var plot = plots[index];
            if (plot.Key.Ward == ward && plot.IsSubdivision == wanted)
            {
                return true;
            }
        }

        return false;
    }

    private void SwitchDivision()
    {
        showSubdivision = !showSubdivision;
        sheetOpen = false;
        selectedPlot = default;
        ResetMapView();
        InvalidateCache();
    }

    private static Rect? DivisionSwitchRect(Rect viewport, in HousingPlan plan, float scale)
    {
        if (!plan.HasDivisions)
        {
            return null;
        }

        var height = 26f * scale;
        var width = MathF.Min(viewport.Width - 100f * scale,
            Typography.Measure(Loc.T(L.Housing.MainDivision), TextStyles.SubheadlineEmphasized).X +
            Typography.Measure(Loc.T(L.Housing.Subdivision), TextStyles.SubheadlineEmphasized).X + 46f * scale);
        var center = new Vector2(viewport.Center.X, viewport.Min.Y + 16f * scale + height * 0.5f);
        return new Rect(new Vector2(center.X - width * 0.5f, center.Y - height * 0.5f),
            new Vector2(center.X + width * 0.5f, center.Y + height * 0.5f));
    }

    private void DrawDivisionSwitch(Rect viewport, in HousingPlan plan, float scale)
    {
        if (DivisionSwitchRect(viewport, plan, scale) is not { } rect)
        {
            return;
        }

        var mainLabel = Loc.T(L.Housing.MainDivision);
        var subLabel = Loc.T(L.Housing.Subdivision);
        var height = rect.Height;
        Elevation.Floating(ImGui.GetWindowDrawList(), rect.Min, rect.Max, height * 0.5f, scale, 0.7f);
        var picked = HousingChrome.Segment(rect, mainLabel, subLabel, showSubdivision ? 1 : 0, false);
        if (picked == 1 == showSubdivision)
        {
            return;
        }

        SwitchDivision();
    }

    private bool WardHasReportedPlots()
    {
        if (housing.Snapshot is not { } snapshot)
        {
            return false;
        }

        var ward = housing.Ward;
        var plots = snapshot.Plots;
        for (var index = 0; index < plots.Count; index++)
        {
            if (plots[index].Key.Ward == ward)
            {
                return true;
            }
        }

        return false;
    }

    private void DrawLoading(Rect viewport, string label, float scale)
    {
        LoadingPulse.Draw(new Vector2(viewport.Center.X, viewport.Center.Y - 12f * scale), 18f * scale, PlacardTheme.Accent,
            PlacardTheme.MutedInk, label);
    }

    private void DrawOfflineState(Rect viewport, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var width = MathF.Min(viewport.Width - 48f * scale, 300f * scale);
        var height = 176f * scale;
        var center = viewport.Center;
        var min = new Vector2(center.X - width * 0.5f, center.Y - height * 0.5f);
        var max = new Vector2(center.X + width * 0.5f, center.Y + height * 0.5f);
        PopoverSurface.Draw(drawList, min, max, Metrics.Radius.Card * scale, scale);
        HousingGlyphs.Estate(drawList, new Vector2(center.X, min.Y + 36f * scale), 16f * scale, PlacardTheme.MutedInk,
            PlacardTheme.Backdrop);
        Typography.DrawCentered(drawList, new Vector2(center.X, min.Y + 76f * scale), Loc.T(L.Housing.Offline),
            PlacardTheme.TitleInk, TextStyles.Headline);
        Typography.DrawWrappedCentered(drawList, new Vector2(center.X, min.Y + 108f * scale),
            Loc.T(L.Housing.OfflineHint), PlacardTheme.MutedInk, TextStyles.Footnote, width - 32f * scale);
        var buttonHeight = 30f * scale;
        var buttonY = max.Y - 18f * scale - buttonHeight;
        var retry = new Rect(new Vector2(min.X + 24f * scale, buttonY),
            new Vector2(max.X - 24f * scale, buttonY + buttonHeight));
        if (HousingChrome.PillButton(retry, Loc.T(L.Housing.Retry), true, false))
        {
            RequestRefresh();
        }
    }

    private void DrawNoWorldState(Rect viewport, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var width = MathF.Min(viewport.Width - 48f * scale, 300f * scale);
        var height = 190f * scale;
        var center = viewport.Center;
        var min = new Vector2(center.X - width * 0.5f, center.Y - height * 0.5f);
        var max = new Vector2(center.X + width * 0.5f, center.Y + height * 0.5f);
        PopoverSurface.Draw(drawList, min, max, Metrics.Radius.Card * scale, scale);
        HousingGlyphs.Estate(drawList, new Vector2(center.X, min.Y + 38f * scale), 18f * scale, PlacardTheme.Accent,
            PlacardTheme.Backdrop);
        Typography.DrawCentered(drawList, new Vector2(center.X, min.Y + 84f * scale), Loc.T(L.Housing.NoWorldTitle),
            PlacardTheme.TitleInk, TextStyles.Headline);
        Typography.DrawWrappedCentered(drawList, new Vector2(center.X, min.Y + 116f * scale),
            Loc.T(L.Housing.NoWorldHint), PlacardTheme.MutedInk, TextStyles.Footnote, width - 32f * scale);
        var buttonHeight = 32f * scale;
        var chooseY = max.Y - 18f * scale - buttonHeight;
        var choose = new Rect(new Vector2(min.X + 20f * scale, chooseY),
            new Vector2(max.X - 20f * scale, chooseY + buttonHeight));
        if (HousingChrome.PillButton(choose, Loc.T(L.Housing.ChooseWorld), true, false))
        {
            worldSearch = string.Empty;
            OpenWorldPicker();
        }
    }

    private void DrawEmptyCard(Rect viewport, FontAwesomeIcon icon, string title, string? hint, string actionLabel,
        Action onAction, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var width = MathF.Min(viewport.Width - 48f * scale, 300f * scale);
        var hintHeight = hint is null
            ? 0f
            : Typography.MeasureWrappedBlock(hint, TextStyles.Footnote, width - 32f * scale).Y + 10f * scale;
        var height = 148f * scale + hintHeight;
        var center = new Vector2(viewport.Center.X, viewport.Center.Y - 10f * scale);
        var min = new Vector2(center.X - width * 0.5f, center.Y - height * 0.5f);
        var max = new Vector2(center.X + width * 0.5f, center.Y + height * 0.5f);
        PopoverSurface.Draw(drawList, min, max, Metrics.Radius.Card * scale, scale);
        var iconCenter = new Vector2(center.X, min.Y + 34f * scale);
        drawList.AddCircleFilled(iconCenter, 20f * scale, ImGui.GetColorU32(PlacardTheme.FieldSurface), 28);
        Icons.DrawCentered(drawList, iconCenter, icon, PlacardTheme.MutedInk, 1f);
        var titleY = min.Y + 70f * scale;
        Typography.DrawWrappedCentered(drawList, new Vector2(center.X, titleY + 8f * scale), title, PlacardTheme.TitleInk,
            TextStyles.SubheadlineEmphasized, width - 28f * scale);
        if (hint is not null)
        {
            Typography.DrawWrappedCentered(drawList, new Vector2(center.X, titleY + 34f * scale + hintHeight * 0.2f),
                hint, PlacardTheme.MutedInk, TextStyles.Footnote, width - 32f * scale);
        }

        var buttonHeight = 30f * scale;
        var buttonY = max.Y - 16f * scale - buttonHeight;
        var button = new Rect(new Vector2(min.X + 24f * scale, buttonY),
            new Vector2(max.X - 24f * scale, buttonY + buttonHeight));
        if (HousingChrome.PillButton(button, actionLabel, true, false))
        {
            onAction();
        }
    }

    private Rect? FirstUseHintRect(Rect viewport, List<HousingPlot> plots, float scale)
    {
        if (configuration.HousingMapHintDismissed || plots.Count == 0 || sheetOpen)
        {
            return null;
        }

        var width = MathF.Min(viewport.Width - 60f * scale, 280f * scale);
        var textSize = Typography.MeasureWrappedBlock(Loc.T(L.Housing.MapHint), TextStyles.Footnote,
            width - 26f * scale);
        var height = textSize.Y + 24f * scale + 24f * scale;
        var min = new Vector2(viewport.Center.X - width * 0.5f, viewport.Max.Y - height - 14f * scale);
        return new Rect(min, new Vector2(min.X + width, min.Y + height));
    }

    private void DrawFirstUseHint(Rect viewport, List<HousingPlot> plots, float scale)
    {
        if (FirstUseHintRect(viewport, plots, scale) is not { } bounds)
        {
            return;
        }

        var drawList = ImGui.GetWindowDrawList();
        var width = bounds.Width;
        var text = Loc.T(L.Housing.MapHint);
        var dismissHeight = 24f * scale;
        var min = bounds.Min;
        var max = bounds.Max;
        var rounding = Metrics.Radius.Md * scale;
        Elevation.Card(drawList, min, max, rounding, scale);
        Squircle.Fill(drawList, min, max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.SurfaceMuted, 0.96f)));
        Squircle.Stroke(drawList, min, max, rounding,
            ImGui.GetColorU32(Palette.WithAlpha(PlacardTheme.Brass, 0.34f)), Metrics.Stroke.Hairline);
        Typography.DrawWrappedLeft(new Vector2(min.X + 13f * scale, min.Y + 11f * scale), text, PlacardTheme.BodyInk,
            TextStyles.Footnote, width - 26f * scale);
        var dismissWidth = HousingChrome.MeasurePill(Loc.T(L.Housing.GotIt), dismissHeight);
        var dismiss = new Rect(new Vector2(max.X - 13f * scale - dismissWidth, max.Y - 11f * scale - dismissHeight),
            new Vector2(max.X - 13f * scale, max.Y - 11f * scale));
        if (HousingChrome.PillButton(dismiss, Loc.T(L.Housing.GotIt), false, true))
        {
            configuration.HousingMapHintDismissed = true;
            configuration.Save();
        }
    }

    private void DrawLegend(Rect viewport, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var entries = LegendEntries;
        var rowHeight = 20f * scale;
        var width = 178f * scale;
        var height = entries.Length * rowHeight + 18f * scale;
        var min = new Vector2(viewport.Min.X + 14f * scale, viewport.Min.Y + 50f * scale);
        var max = new Vector2(min.X + width, min.Y + height);
        var rounding = Metrics.Radius.Md * scale;
        PopoverSurface.Draw(drawList, min, max, rounding, scale);
        for (var index = 0; index < entries.Length; index++)
        {
            var rowCenterY = min.Y + 9f * scale + rowHeight * (index + 0.5f);
            var swatchCenter = new Vector2(min.X + 20f * scale, rowCenterY);
            DrawLegendSwatch(drawList, swatchCenter, index, scale);
            Typography.Draw(drawList,
                new Vector2(min.X + 38f * scale, rowCenterY - Typography.LineHeight(TextStyles.Caption1) * 0.5f),
                Loc.T(entries[index]), PlacardTheme.BodyInk, TextStyles.Caption1);
        }
    }

    private void DrawLegendSwatch(ImDrawListPtr drawList, Vector2 center, int index, float scale)
    {
        switch (index)
        {
            case 0:
                HousingMarkers.DrawSwatch(drawList, center, HousingPlotSize.Small, PlacardTheme.Accent, scale);
                break;
            case 1:
                HousingMarkers.DrawSwatch(drawList, center, HousingPlotSize.Medium, PlacardTheme.Accent, scale);
                break;
            case 2:
                HousingMarkers.DrawSwatch(drawList, center, HousingPlotSize.Large, PlacardTheme.Accent, scale);
                break;
            case 3:
                HousingMarkers.DrawSwatch(drawList, center, HousingPlotSize.Small, PlacardTheme.MutedInk, scale);
                HousingGlyphs.WatchNotch(drawList, center, 5f * scale, ImGui.GetColorU32(PlacardTheme.Brass));
                break;
            case 4:
                HousingGlyphs.DashedRing(drawList, center, 7f * scale,
                    ImGui.GetColorU32(PlacardTheme.Parchment), 1.4f * scale);
                break;
            default:
                drawList.AddCircle(center, 7.5f * scale, ImGui.GetColorU32(new Vector4(1f, 1f, 1f, 1f)), 24,
                    1.8f * scale);
                break;
        }
    }

    private static readonly LocString[] LegendEntries =
    {
        L.Housing.LegendSmall, L.Housing.LegendMedium, L.Housing.LegendLarge, L.Housing.LegendWatched,
        L.Housing.LegendStale, L.Housing.LegendSelected,
    };

    private string FooterStatusText()
    {
        if (housing.IsRefreshing || refreshFeedback)
        {
            return Loc.T(L.Housing.Updating);
        }

        if (housing.Snapshot is not { } snapshot)
        {
            return Loc.T(L.Housing.Offline);
        }

        return Loc.T(L.Housing.UpdatedAgo, HousingFormat.ScanAgeShort(snapshot.FetchedUtc, DateTime.UtcNow));
    }

    private HousingDataFreshness FooterFreshness()
    {
        if (housing.Snapshot is not { } snapshot)
        {
            return HousingDataFreshness.Unknown;
        }

        return housing.Thresholds.Classify(snapshot.FetchedUtc, DateTime.UtcNow, snapshot.Source);
    }
}
