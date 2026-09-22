using Placard.Core;
using Placard.Core.Housing;
using Placard.Core.Localization;
using Placard.Core.Theme;
using Placard.Windows.Components;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;

namespace Placard.Windows.Housing;

internal sealed partial class HousingApp
{
    private void DrawPlotPanelContent(Rect content, HousingPlot plot, float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var contentLeft = content.Min.X;
        var contentRight = content.Max.X;
        var contentWidth = contentRight - contentLeft;
        var y = content.Min.Y;

        var place = string.Concat(HousingFormat.Place(housing.DistrictName, plot.Key.Ward), " \u00b7 ",
            HousingFormat.DivisionLabel(plot.IsSubdivision));
        Typography.Draw(drawList, new Vector2(contentLeft, y),
            Typography.FitText(place, contentWidth, TextStyles.Footnote), PlacardTheme.MutedInk, TextStyles.Footnote);
        y += Typography.LineHeight(TextStyles.Footnote) + 9f * scale;

        var freshness = FreshnessOf(plot);
        var chipX = contentLeft;
        var chipLabel = HousingFormat.FreshnessLabel(freshness);
        HousingChrome.Chip(drawList, new Vector2(chipX, y), chipLabel,
            HousingChrome.FreshnessHue(freshness, PlacardTheme.Accent), false);
        chipX += HousingChrome.MeasureChip(chipLabel) + 6f * scale;
        var phaseLabel = HousingFormat.PhaseLabel(plot.Phase);
        HousingChrome.Chip(drawList, new Vector2(chipX, y), phaseLabel,
            HousingMarkers.PhaseColor(plot.Phase, PlacardTheme.Accent), false);
        chipX += HousingChrome.MeasureChip(phaseLabel) + 6f * scale;
        if (housing.Watch.IsWatched(plot.Key))
        {
            HousingChrome.Chip(drawList, new Vector2(chipX, y), Loc.T(L.Housing.Watching),
                PlacardTheme.Brass, false);
        }

        y += HousingChrome.ChipHeight * scale + 10f * scale;

        var now = DateTime.UtcNow;
        var countdown = HousingFormat.PhaseCountdown(plot, now);
        Typography.Draw(drawList, new Vector2(contentLeft, y),
            Loc.Culture.TextInfo.ToUpper(HousingFormat.PhaseLabel(plot.Phase)), PlacardTheme.HeaderInk,
            TextStyles.Caption2);
        Typography.Draw(drawList, new Vector2(contentLeft, y + 11f * scale),
            Typography.FitText(countdown, contentWidth, TextStyles.Title3), PlacardTheme.TitleInk, TextStyles.Title3);
        y += Typography.LineHeight(TextStyles.Caption2) + 2f * scale + Typography.LineHeight(TextStyles.Title3) +
             8f * scale;
        var scanText = HousingFormat.ScanAge(plot.LastSeenUtc, now);
        Typography.Draw(drawList, new Vector2(contentLeft, y),
            Typography.FitText(scanText, contentWidth, TextStyles.Footnote),
            freshness == HousingDataFreshness.Stale ? PlacardTheme.Results : PlacardTheme.MutedInk,
            TextStyles.Footnote);
        y += Typography.LineHeight(TextStyles.Footnote) + 12f * scale;

        var rowHeight = HousingChrome.StatRowHeight * scale;
        HousingChrome.StatRow(drawList, new Rect(new Vector2(contentLeft, y), new Vector2(contentRight, y + rowHeight)),
            Loc.T(L.Housing.EntriesLabel), HousingFormat.Entries(plot.Entries), true);
        y += rowHeight;
        HousingChrome.StatRow(drawList, new Rect(new Vector2(contentLeft, y), new Vector2(contentRight, y + rowHeight)),
            Loc.T(L.Housing.PriceLabel), HousingFormat.Price(plot.Price));
        y += rowHeight;
        HousingChrome.StatRow(drawList, new Rect(new Vector2(contentLeft, y), new Vector2(contentRight, y + rowHeight)),
            Loc.T(L.Housing.EligibilityLabel), HousingFormat.EligibilityLabel(plot.Eligibility));
        y += rowHeight + 10f * scale;

        if (reminderPickerOpen)
        {
            DrawReminderPicker(drawList, plot, contentLeft, contentRight, y, scale);
            return;
        }

        DrawSheetActions(plot, contentLeft, contentRight, y, scale);
    }

    private void DrawSheetActions(HousingPlot plot, float left, float right, float travelTop, float scale)
    {
        var height = 32f * scale;
        var gap = 8f * scale;
        var travelRow = new Rect(new Vector2(left, travelTop), new Vector2(right, travelTop + height));
        if (HousingChrome.PillButton(travelRow, Loc.T(L.Housing.TravelHere), true, false, HousingTravel.IsAvailable))
        {
            TravelTo(plot.Key);
        }

        var watched = housing.Watch.IsWatched(plot.Key);
        var reminder = housing.Watch.FindReminder(plot.Key);
        var hasDeadline = plot.PhaseEndsUtc is not null;
        var watchLabel = Loc.T(watched ? L.Housing.Watching : L.Housing.Watch);
        var remindLabel = reminder is { Notified: false }
            ? Loc.T(L.Housing.ReminderSet)
            : Loc.T(L.Housing.RemindMe);

        var watchRect = new Rect(new Vector2(left, travelRow.Max.Y + gap),
            new Vector2(right, travelRow.Max.Y + gap + height));
        var remindRect = new Rect(new Vector2(left, watchRect.Max.Y + gap),
            new Vector2(right, watchRect.Max.Y + gap + height));
        var detailsRect = new Rect(new Vector2(left, remindRect.Max.Y + gap),
            new Vector2(right, remindRect.Max.Y + gap + height));

        if (HousingChrome.PillButton(watchRect, watchLabel, watched, false))
        {
            var nowWatched = housing.Watch.ToggleWatch(plot, housing.WorldNameOf(plot.Key.WorldId));
            ShowToast(Loc.T(nowWatched ? L.Housing.Watching : L.Housing.Unwatch));
            InvalidateCache();
        }

        if (HousingChrome.PillButton(remindRect, remindLabel, reminder is { Notified: false }, false, hasDeadline))
        {
            ShowOverlay(HousingOverlay.ReminderPicker);
            reminderChoice = IndexOfLeadTime(reminder?.OffsetMinutes ?? configuration.HousingReminderMinutes);
        }

        if (!hasDeadline)
        {
            HoverTooltip.Show("housing.remind.disabled", remindRect, Loc.T(L.Housing.ReminderUnavailable),
                HoverLabelSide.Above);
        }

        if (HousingChrome.PillButton(detailsRect, Loc.T(L.Housing.DetailsAction), false, false))
        {
            Push(HousingRoute.Details, plot.Key);
        }
    }

    private void DrawReminderPicker(ImDrawListPtr drawList, HousingPlot plot, float left, float right, float top,
        float scale)
    {
        HousingChrome.SectionLabel(drawList, new Vector2(left, top), right - left, Loc.T(L.Housing.ReminderPrompt));
        var y = top + Typography.LineHeight(TextStyles.Caption1) + 6f * scale;
        var choices = HousingDefaults.ReminderChoices;
        for (var index = 0; index < choices.Length; index++)
        {
            reminderLabels[index] = HousingFormat.LeadTime(choices[index]);
            reminderActive[index] = index == reminderChoice;
        }

        var leadRow = new Rect(new Vector2(left, y), new Vector2(right, y + ChipRail.RowHeight * scale));
        var leadTapped = reminderRail.Draw(leadRow, reminderLabels, reminderActive, true);
        if (leadTapped >= 0)
        {
            reminderChoice = leadTapped;
        }

        var buttonHeight = 32f * scale;
        var buttonTop = leadRow.Max.Y + 10f * scale;
        var gap = 8f * scale;
        var existing = housing.Watch.FindReminder(plot.Key);
        var confirmLabel = Loc.T(L.Housing.ReminderSet);
        var removeLabel = Loc.T(L.Housing.CancelReminder);
        var dismissLabel = Loc.T(L.Common.Cancel);
        var buttonRow = new Rect(new Vector2(left, buttonTop), new Vector2(right, buttonTop + buttonHeight));
        Span<Rect> rects = stackalloc Rect[3];
        Rect confirmRect;
        Rect? cancelReminderRect = null;
        Rect dismissRect;
        if (existing is null)
        {
            Span<string> twoLabels = [confirmLabel, dismissLabel];
            HousingChrome.LayoutPills(buttonRow, twoLabels, gap, rects);
            confirmRect = rects[0];
            dismissRect = rects[1];
        }
        else
        {
            Span<string> threeLabels = [confirmLabel, removeLabel, dismissLabel];
            HousingChrome.LayoutPills(buttonRow, threeLabels, gap, rects);
            confirmRect = rects[0];
            cancelReminderRect = rects[1];
            dismissRect = rects[2];
        }

        if (HousingChrome.PillButton(confirmRect, confirmLabel, true, true))
        {
            var minutes = choices[Math.Clamp(reminderChoice, 0, choices.Length - 1)];
            if (housing.Watch.SetReminder(plot, housing.WorldNameOf(plot.Key.WorldId), minutes))
            {
                configuration.HousingReminderMinutes = minutes;
                configuration.Save();
                reminderPickerOpen = false;
                ShowToast(Loc.T(L.Housing.ReminderConfirmed, HousingFormat.LeadTime(minutes),
                    HousingFormat.PhaseLabel(plot.Phase),
                    HousingFormat.Place(HousingDistricts.DisplayName(plot.Key.DistrictId), plot.Key.Ward),
                    plot.Key.Plot));
            }
            else
            {
                ShowToast(Loc.T(L.Housing.ReminderUnavailable));
            }
        }

        if (cancelReminderRect is { } cancelRect &&
            HousingChrome.PillButton(cancelRect, removeLabel, false, true))
        {
            housing.Watch.CancelReminder(plot.Key);
            reminderPickerOpen = false;
            ShowToast(Loc.T(L.Housing.CancelReminder));
        }

        if (HousingChrome.PillButton(dismissRect, dismissLabel, false, true))
        {
            reminderPickerOpen = false;
        }
    }

    private void DrawWardPicker(Rect area, Rect viewport, float scale)
    {
        if (!wardPickerOpen)
        {
            return;
        }

        var drawList = ImGui.GetWindowDrawList();
        var district = HousingDistricts.Resolve(housing.DistrictId);
        const int columns = 5;
        var rows = (district.Wards + columns - 1) / columns;
        var pad = Selection.PanelPadding * scale;
        var gap = 7f * scale;
        var cell = MathF.Min(46f * scale, (viewport.Width - pad * 2f - (columns - 1) * gap) / columns);
        cell = MathF.Max(cell, 30f * scale);
        var legendHeight = Typography.LineHeight(Selection.LegendStyle) + 12f * scale;
        var width = columns * cell + (columns - 1) * gap + pad * 2f;
        var gridHeight = rows * cell + (rows - 1) * gap;
        var height = pad * 2f + Selection.TitleHeight(scale) + gridHeight + legendHeight;
        var center = new Vector2(viewport.Center.X, viewport.Center.Y);
        var min = new Vector2(center.X - width * 0.5f, center.Y - height * 0.5f);
        var panel = new Rect(min, new Vector2(min.X + width, min.Y + height));

        Veil.Draw(drawList, area.Min, area.Max, 0.34f);
        Selection.Backdrop(drawList, panel, scale);
        var gridTop = Selection.Title(drawList, panel, Loc.T(L.Housing.ChooseWard), panel.Min.Y + pad, scale);

        Span<int> counts = stackalloc int[district.Wards];
        housing.CollectWardOpenings(counts);
        var current = housing.Ward;
        for (var index = 0; index < district.Wards; index++)
        {
            var column = index % columns;
            var row = index / columns;
            var cellMin = new Vector2(panel.Min.X + pad + column * (cell + gap), gridTop + row * (cell + gap));
            var bounds = new Rect(cellMin, cellMin + new Vector2(cell, cell));
            var ward = index + 1;
            var selected = ward == current;
            var hovered = UiInteract.HoverWindowOnly(bounds.Min, bounds.Max, false);
            if (!selected && !hovered)
            {
                Squircle.Fill(drawList, bounds.Min, bounds.Max, Selection.Radius(scale),
                    ImGui.GetColorU32(PlacardTheme.FieldSurface));
            }

            Selection.Surface(drawList, bounds, selected, hovered, true, scale);
            var ink = Selection.Ink(selected, hovered, true);
            Typography.DrawCentered(drawList, new Vector2(bounds.Center.X, bounds.Center.Y - 4f * scale),
                ward.ToString(Loc.Culture), ink, TextStyles.SubheadlineEmphasized);
            if (counts[index] > 0)
            {
                Selection.Marker(drawList, new Vector2(bounds.Center.X, bounds.Max.Y - 9f * scale), scale);
            }

            if (hovered)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            if (!UiInteract.ClickImmediate(bounds.Min, bounds.Max, hovered))
            {
                continue;
            }

            housing.SelectWard(ward);
            wardPickerOpen = false;
            sheetOpen = false;
            selectedPlot = default;
            InvalidateCache();
        }

        var legendY = gridTop + gridHeight + 9f * scale;
        var legend = Loc.T(L.Housing.WardLegend);
        var legendSize = Typography.Measure(legend, Selection.LegendStyle);
        var markerX = panel.Center.X - (legendSize.X + 12f * scale) * 0.5f;
        Selection.Marker(drawList, new Vector2(markerX, legendY + legendSize.Y * 0.5f), scale);
        Typography.Draw(drawList, new Vector2(markerX + 9f * scale, legendY), legend, PlacardTheme.MutedInk,
            Selection.LegendStyle);

        if (UiInteract.ClickedOutside(panel.Min, panel.Max, false))
        {
            wardPickerOpen = false;
        }
    }
}
