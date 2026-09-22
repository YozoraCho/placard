using Placard.Core;
using Placard.Core.Housing;
using Placard.Core.Localization;
using Placard.Core.Theme;
using Placard.Windows.Components;
using Dalamud.Bindings.ImGui;

namespace Placard.Windows.Housing;

internal sealed partial class HousingApp
{
    private const float SettingsRowHeight = Metrics.Size.Row;

    private void DrawSettingsRoute(Rect area)
    {
        var scale = UiScale.Current;
        var body = DrawSubHeader(area, "housing.header.settings", Loc.T(L.Housing.Settings));
        using (AppSurface.Begin(body))
        {
            DrawWorldSettings(scale);
            DrawDataSettings(scale);
            DrawReminderSettings(scale);
            DrawMapSettings(scale);
            DrawDiagnostics(scale);
            ImGui.Dummy(new Vector2(0f, 30f * scale));
        }
    }

    private void DrawWorldSettings(float scale)
    {
        SettingsSection.Header(Loc.T(L.Housing.SettingsWorld));
        var card = GroupCard.Begin(2, SettingsRowHeight);
        var worldName = housing.WorldName;
        if (SettingsRow.Disclosure(card.NextRow(), Loc.T(L.Housing.PreferredWorld),
                worldName.Length > 0 ? worldName : Loc.T(L.Housing.ChooseWorld)))
        {
            worldSearch = string.Empty;
            OpenWorldPicker();
        }

        var follow = SettingsRow.Bool(card.NextRow(), Loc.T(L.Housing.FollowCurrentWorld),
            configuration.HousingFollowCurrentWorld);
        if (follow != configuration.HousingFollowCurrentWorld)
        {
            housing.SetFollowCurrentWorld(follow);
        }

        card.End();
        SettingsSection.Hint(Loc.T(L.Housing.FollowCurrentWorldHint));
    }

    private void DrawDataSettings(float scale)
    {
        SettingsSection.Header(Loc.T(L.Housing.SettingsData));
        var card = GroupCard.Begin(4, SettingsRowHeight);
        var autoRefresh = SettingsRow.Bool(card.NextRow(), Loc.T(L.Housing.AutoRefresh),
            configuration.HousingAutoRefresh);
        if (autoRefresh != configuration.HousingAutoRefresh)
        {
            configuration.HousingAutoRefresh = autoRefresh;
            configuration.Save();
        }

        var interval = DrawNumberRow(card.NextRow(), Loc.T(L.Housing.RefreshInterval), "##housingRefreshMinutes",
            housing.RefreshMinutes, HousingDefaults.MinRefreshMinutes, HousingDefaults.MaxRefreshMinutes,
            HousingDefaults.RefreshStepMinutes, scale);
        if (interval != housing.RefreshMinutes)
        {
            housing.SetRefreshMinutes(interval);
        }

        var live = DrawNumberRow(card.NextRow(), Loc.T(L.Housing.FreshnessThreshold), "##housingLiveMinutes",
            configuration.HousingLiveMinutes, 5, 45, 5, scale);
        if (live != configuration.HousingLiveMinutes)
        {
            SetFreshnessMinutes(live);
        }

        if (SettingsRow.Action(card.NextRow(), Loc.T(L.Housing.ClearCache), PlacardTheme.Danger))
        {
            confirm.Ask(new ConfirmRequest
            {
                Title = Loc.T(L.Housing.SettingsData),
                Message = Loc.T(L.Housing.ClearCache),
                ConfirmLabel = Loc.T(L.Housing.ClearCache),
                CancelLabel = Loc.T(L.Common.Cancel),
                Confirm = () =>
                {
                    housing.ClearCache();
                    InvalidateCache();
                },
            });
        }

        card.End();
        SettingsSection.Hint(Loc.T(L.Housing.RefreshIntervalHint, HousingDefaults.MinRefreshMinutes));
    }

    private void DrawReminderSettings(float scale)
    {
        SettingsSection.Header(Loc.T(L.Housing.SettingsNotifications));
        var card = GroupCard.Begin(3, SettingsRowHeight);
        var notifyEntry = SettingsRow.Bool(card.NextRow(), Loc.T(L.Housing.NotifyEntry),
            configuration.HousingNotifyEntry);
        if (notifyEntry != configuration.HousingNotifyEntry)
        {
            configuration.HousingNotifyEntry = notifyEntry;
            configuration.Save();
        }

        var notifyResults = SettingsRow.Bool(card.NextRow(), Loc.T(L.Housing.NotifyResults),
            configuration.HousingNotifyResults);
        if (notifyResults != configuration.HousingNotifyResults)
        {
            configuration.HousingNotifyResults = notifyResults;
            configuration.Save();
        }

        var lead = DrawNumberRow(card.NextRow(), Loc.T(L.Housing.ReminderLead), "##housingLeadMinutes",
            configuration.HousingReminderMinutes, 1, 240, 5, scale);
        if (lead != configuration.HousingReminderMinutes)
        {
            configuration.HousingReminderMinutes = lead;
            configuration.Save();
        }

        card.End();
        SettingsSection.Hint(Loc.T(L.Housing.ReminderLeadHint));
    }

    private void DrawMapSettings(float scale)
    {
        SettingsSection.Header(Loc.T(L.Housing.SettingsMap));
        var card = GroupCard.Begin(2, SettingsRowHeight);
        var allPlots = SettingsRow.Bool(card.NextRow(), Loc.T(L.Housing.ShowAllPlots), housing.Filters.ShowAllPlots);
        if (allPlots != housing.Filters.ShowAllPlots)
        {
            housing.Filters.ShowAllPlots = allPlots;
            housing.PersistFilterDefaults();
            InvalidateCache();
        }

        if (SettingsRow.Action(card.NextRow(), Loc.T(L.Housing.ResetMap), PlacardTheme.Accent))
        {
            ResetMapView();
            configuration.HousingMapHintDismissed = false;
            configuration.Save();
        }

        card.End();
        SettingsSection.Hint(
            housing.GameMap is null
                ? Loc.T(L.Housing.GameMapUnavailableDetail, housing.GameMapDetail)
                : Loc.T(L.Housing.GameMapHint));
    }

    private void DrawDiagnostics(float scale)
    {
        SettingsSection.Header(Loc.T(L.Housing.SettingsDiagnostics));
        var snapshot = housing.Snapshot;
        var gameMap = housing.GameMap;
        var proxyAge = housing.ProxyCacheAgeSeconds;
#if DEBUG
        var rows = proxyAge is null ? 6 : 7;
#else
        var rows = proxyAge is null ? 5 : 6;
#endif
        var card = GroupCard.Begin(rows, SettingsRowHeight);
        SettingsRow.Info(card.NextRow(), Loc.T(L.Housing.MapSourceLabel),
            gameMap is null
                ? string.Concat(Loc.T(L.Housing.GameMapUnavailable), " · ", housing.GameMapFailure.ToString())
                : gameMap.Main.MapId);
        SettingsRow.Info(card.NextRow(), Loc.T(L.Housing.ProviderStatus), housing.ProviderName);
        if (proxyAge is { } age)
        {
            SettingsRow.Info(card.NextRow(), Loc.T(L.Housing.ProxyCacheAge),
                HousingFormat.Countdown(TimeSpan.FromSeconds(age)));
        }

        SettingsRow.Info(card.NextRow(), Loc.T(L.Housing.LastRefresh),
            housing.LastSuccessUtc == default
                ? Loc.T(L.Housing.NotReported)
                : HousingFormat.ExactLocalTime(housing.LastSuccessUtc));
        SettingsRow.Info(card.NextRow(), Loc.T(L.Housing.OpenPlotsReported),
            snapshot is null ? Loc.T(L.Housing.NotReported) : snapshot.OpenPlotCount.ToString(Loc.Culture));
        SettingsRow.Info(card.NextRow(), Loc.T(L.Housing.StatusLabel),
            housing.LastError ?? HousingFormat.FreshnessLabel(FooterFreshness()));
#if DEBUG
        if (SettingsRow.Action(card.NextRow(), Loc.T(L.Housing.CopyMapDiagnostics), PlacardTheme.Accent))
        {
            ImGui.SetClipboardText(housing.DescribeGameMap());
            ShowToast(Loc.T(L.Housing.CopiedMapDiagnostics));
        }
#endif

        card.End();
        SettingsSection.Hint(Loc.T(L.Housing.DataSourceNotice));
    }

    private int DrawNumberRow(Rect row, string label, string id, int value, int minimum, int maximum, int step,
        float scale)
    {
        var drawList = ImGui.GetWindowDrawList();
        var controlWidth = 146f * scale;
        var labelMax = MathF.Max(1f, row.Width - controlWidth - 12f * scale);
        Typography.Draw(drawList, new Vector2(row.Min.X, row.Center.Y - Typography.LineHeight(TextStyles.Body) * 0.5f),
            Typography.FitText(label, labelMax, TextStyles.Body), PlacardTheme.TitleInk, TextStyles.Body);
        var height = 28f * scale;
        var control = new Rect(new Vector2(row.Max.X - controlWidth, row.Center.Y - height * 0.5f),
            new Vector2(row.Max.X, row.Center.Y + height * 0.5f));
        return HousingChrome.NumberStepper(control, id, value, minimum, maximum, step,
            Loc.T(L.Housing.MinutesSuffix));
    }

    private void SetFreshnessMinutes(int minutes)
    {
        configuration.HousingLiveMinutes = Math.Clamp(minutes, 5, 45);
        configuration.HousingRecentMinutes =
            Math.Max(configuration.HousingLiveMinutes + 15, configuration.HousingRecentMinutes);
        configuration.Save();
        InvalidateCache();
    }
}
