namespace Placard.Core.Localization;

internal static class L
{
    internal static class Common
    {
        public static readonly LocString Cancel = new("common.cancel", "Cancel");
        public static readonly LocString Close = new("common.close", "Close");
        public static readonly LocString Loading = new("common.loading", "Loading…");
    }

    internal static class Housing
    {
        public static readonly LocString Map = new("housing.map", "Map");
        public static readonly LocString List = new("housing.list", "List");
        public static readonly LocString Watchlist = new("housing.watchlist", "Watchlist");
        public static readonly LocString Settings = new("housing.settings", "Placard Settings");
        public static readonly LocString Details = new("housing.details", "Plot Details");
        public static readonly LocString ChooseWorld = new("housing.chooseWorld", "Choose World");
        public static readonly LocString ChooseWard = new("housing.chooseWard", "Choose Another Ward");
        public static readonly LocString WardLegend = new("housing.wardLegend",
            "Wards with open plots");
        public static readonly LocString SelectDistrictTitle =
            new("housing.selectDistrictTitle", "Select a district");
        public static readonly LocString SortByTitle = new("housing.sortByTitle", "Sort by");
        public static readonly LocString ViewAsList = new("housing.viewAsList", "View as List");
        public static readonly LocString BackToMap = new("housing.backToMap", "Back to Map");
        public static readonly LocString WorldLabel = new("housing.worldLabel", "World");
        public static readonly LocString DistrictLabel = new("housing.districtLabel", "District");
        public static readonly LocString DistrictMist = new("housing.districtMist", "Mist");
        public static readonly LocString DistrictMistShort = new("housing.districtMistShort", "Mist");
        public static readonly LocString DistrictLavenderBeds =
            new("housing.districtLavenderBeds", "The Lavender Beds");
        public static readonly LocString DistrictLavenderBedsShort =
            new("housing.districtLavenderBedsShort", "Lavender");
        public static readonly LocString DistrictGoblet = new("housing.districtGoblet", "The Goblet");
        public static readonly LocString DistrictGobletShort = new("housing.districtGobletShort", "Goblet");
        public static readonly LocString DistrictShirogane = new("housing.districtShirogane", "Shirogane");
        public static readonly LocString DistrictShiroganeShort =
            new("housing.districtShiroganeShort", "Shirogane");
        public static readonly LocString DistrictEmpyreum = new("housing.districtEmpyreum", "Empyreum");
        public static readonly LocString DistrictEmpyreumShort = new("housing.districtEmpyreumShort", "Empyreum");
        public static readonly LocString WardLabel = new("housing.wardLabel", "Ward");
        public static readonly LocString WardNumber = new("housing.wardNumber", "Ward {0}");
        public static readonly LocString PlotNumber = new("housing.plotNumber", "Plot {0}");
        public static readonly LocString PlotTitle = new("housing.plotTitle", "Plot {0} ({1})");
        public static readonly LocString PlaceLine = new("housing.placeLine", "{0}, Ward {1}");
        public static readonly LocString SelectWorldTitle = new("housing.selectWorldTitle", "Select a world");
        public static readonly LocString SearchWorlds = new("housing.searchWorlds", "Search worlds");
        public static readonly LocString NoWorldMatches = new("housing.noWorldMatches", "No worlds match that");
        public static readonly LocString HomeWorld = new("housing.homeWorld", "Home world");

        public static readonly LocString SizeSmall = new("housing.sizeSmall", "Small");
        public static readonly LocString SizeMedium = new("housing.sizeMedium", "Medium");
        public static readonly LocString SizeLarge = new("housing.sizeLarge", "Large");
        public static readonly LocString SizeUnknown = new("housing.sizeUnknown", "Unknown size");
        public static readonly LocString PhaseEntry = new("housing.phaseEntry", "Entry period");
        public static readonly LocString PhaseResults = new("housing.phaseResults", "Results period");
        public static readonly LocString PhaseUnavailable = new("housing.phaseUnavailable", "Unavailable period");
        public static readonly LocString PhaseUnknown = new("housing.phaseUnknown", "Phase unknown");
        public static readonly LocString PhaseExpired = new("housing.phaseExpired", "Expired, refreshing");
        public static readonly LocString EligibilityPrivate = new("housing.eligibilityPrivate", "Private buyers");
        public static readonly LocString EligibilityFreeCompany =
            new("housing.eligibilityFreeCompany", "Free Companies");
        public static readonly LocString EligibilityBoth = new("housing.eligibilityBoth", "Private & Free Company");
        public static readonly LocString ModeLottery = new("housing.modeLottery", "Lottery");
        public static readonly LocString ModeFcfs = new("housing.modeFcfs", "First come, first served");
        public static readonly LocString MainDivision = new("housing.mainDivision", "Main division");
        public static readonly LocString Subdivision = new("housing.subdivision", "Subdivision");
        public static readonly LocString NotReported = new("housing.notReported", "Not reported");
        public static readonly LocString TimeUnknown = new("housing.timeUnknown", "Unknown");
        public static readonly LocString PriceGil = new("housing.priceGil", "{0} gil");
        public static readonly LocString EntriesLabel = new("housing.entriesLabel", "Reported entries");
        public static readonly LocString PriceLabel = new("housing.priceLabel", "Price");
        public static readonly LocString EligibilityLabel = new("housing.eligibilityLabel", "Eligibility");
        public static readonly LocString PurchaseLabel = new("housing.purchaseLabel", "Purchase");
        public static readonly LocString DivisionLabel = new("housing.divisionLabel", "Division");
        public static readonly LocString OddsApproximate =
            new("housing.oddsApproximate", "Approximate odds: 1 in {0}");
        public static readonly LocString EntriesCaveat = new("housing.entriesCaveat",
            "Entry counts reflect the most recently reported scan and may have changed.");
        public static readonly LocString RegionLabel = new("housing.regionLabel", "Region");
        public static readonly LocString DataCenterLabel = new("housing.dataCenterLabel", "Data centre");
        public static readonly LocString FirstReported = new("housing.firstReported", "First reported");
        public static readonly LocString PhaseEndsLabel = new("housing.phaseEndsLabel", "Phase ends");
        public static readonly LocString ScannedLabel = new("housing.scannedLabel", "Last scanned");
        public static readonly LocString ProviderLabel = new("housing.providerLabel", "Data provider");
        public static readonly LocString ExactTime = new("housing.exactTime", "Exact time");
        public static readonly LocString StatusLabel = new("housing.statusLabel", "Status");

        public static readonly LocString CountdownDays = new("housing.countdownDays", "{0}d {1:00}h {2:00}m");
        public static readonly LocString CountdownHours = new("housing.countdownHours", "{0:00}h {1:00}m");
        public static readonly LocString CountdownMinutes = new("housing.countdownMinutes", "{0}m {1:00}s");
        public static readonly LocString CountdownUnderMinute =
            new("housing.countdownUnderMinute", "Under 1 minute");
        public static readonly LocString CountdownEnded = new("housing.countdownEnded", "Ended");
        public static readonly LocString Remaining = new("housing.remaining", "{0} remaining");
        public static readonly LocString ScannedJustNow = new("housing.scannedJustNow", "Scanned just now");
        public static readonly LocString ScannedMinutes = new("housing.scannedMinutes", "Scanned {0} minutes ago");
        public static readonly LocString ScannedHours = new("housing.scannedHours", "Scanned {0} hours ago");
        public static readonly LocString ScannedDays = new("housing.scannedDays", "Scanned {0} days ago");
        public static readonly LocString ScannedUnknown = new("housing.scannedUnknown", "Scan time unknown");
        public static readonly LocString AgeNow = new("housing.ageNow", "now");
        public static readonly LocString AgeMinutes = new("housing.ageMinutes", "{0}m");
        public static readonly LocString AgeHours = new("housing.ageHours", "{0}h");
        public static readonly LocString AgeDays = new("housing.ageDays", "{0}d");
        public static readonly LocString FreshnessLive = new("housing.freshnessLive", "Live");
        public static readonly LocString FreshnessRecent = new("housing.freshnessRecent", "Recent");
        public static readonly LocString FreshnessStale = new("housing.freshnessStale", "Stale");
        public static readonly LocString FreshnessCached = new("housing.freshnessCached", "Cached");
        public static readonly LocString FreshnessUnknown = new("housing.freshnessUnknown", "Unknown");
        public static readonly LocString UpdatedAgo = new("housing.updatedAgo", "Updated {0}");
        public static readonly LocString Updating = new("housing.updating", "Updating…");
        public static readonly LocString CachedBanner = new("housing.cachedBanner",
            "Live updates are unavailable. Showing housing data saved {0}.");
        public static readonly LocString AgeJustNow = new("housing.ageJustNow", "just now");
        public static readonly LocString AgeMinutesAgo = new("housing.ageMinutesAgo", "{0} minutes ago");
        public static readonly LocString AgeHoursAgo = new("housing.ageHoursAgo", "{0} hours ago");
        public static readonly LocString AgeDaysAgo = new("housing.ageDaysAgo", "{0} days ago");

        public static readonly LocString Watch = new("housing.watch", "Watch");
        public static readonly LocString Watching = new("housing.watching", "Watching");
        public static readonly LocString Unwatch = new("housing.unwatch", "Unwatch");
        public static readonly LocString RemindMe = new("housing.remindMe", "Remind Me");
        public static readonly LocString ReminderSet = new("housing.reminderSet", "Reminder Set");
        public static readonly LocString CancelReminder = new("housing.cancelReminder", "Cancel reminder");
        public static readonly LocString DetailsAction = new("housing.detailsAction", "Details");
        public static readonly LocString ReminderPrompt =
            new("housing.reminderPrompt", "Notify me before this phase ends:");
        public static readonly LocPlural LeadMinutes = new("housing.leadMinutes", "{0} minute", "{0} minutes");
        public static readonly LocPlural LeadHours = new("housing.leadHours", "{0} hour", "{0} hours");
        public static readonly LocPlural OpenPlotCount =
            new("housing.openPlotCount", "{0} open plot", "{0} open plots");
        public static readonly LocString ReminderConfirmed = new("housing.reminderConfirmed",
            "Reminder set for {0} before the {1} ends. {2}, plot {3}.");
        public static readonly LocString ReminderUnavailable = new("housing.reminderUnavailable",
            "No phase deadline was reported for this plot, so a reminder cannot be scheduled yet.");
        public static readonly LocString TravelHere = new("housing.travelHere", "Travel Here");
        public static readonly LocString TravelStarted =
            new("housing.travelStarted", "Travelling to {0}, plot {1}.");
        public static readonly LocString TravelBusy = new("housing.travelBusy", "Lifestream is busy right now.");
        public static readonly LocString TravelNotAttuned = new("housing.travelNotAttuned",
            "Attune to that city aetheryte before travelling.");
        public static readonly LocString TravelUnavailable =
            new("housing.travelUnavailable", "Travel is not possible right now.");
        public static readonly LocString TravelNeedsLifestream = new("housing.travelNeedsLifestream",
            "Lifestream is not installed. The travel command was copied instead.");
        public static readonly LocString Filters = new("housing.filters", "Filters");
        public static readonly LocString FiltersCount = new("housing.filtersCount", "Filters ({0})");
        public static readonly LocString ClearFilters = new("housing.clearFilters", "Clear Filters");
        public static readonly LocString Refresh = new("housing.refresh", "Refresh");
        public static readonly LocString Retry = new("housing.retry", "Retry");
        public static readonly LocString ZoomIn = new("housing.zoomIn", "Zoom in");
        public static readonly LocString ZoomOut = new("housing.zoomOut", "Zoom out");
        public static readonly LocString ResetMap = new("housing.resetMap", "Reset map");
        public static readonly LocString Recenter = new("housing.recenter", "Centre on selected plot");
        public static readonly LocString Legend = new("housing.legend", "Legend");
        public static readonly LocString MatchingPlots = new("housing.matchingPlots", "{0} matching");

        public static readonly LocString FilterSizes = new("housing.filterSizes", "Plot size");
        public static readonly LocString FilterPhase = new("housing.filterPhase", "Lottery phase");
        public static readonly LocString FilterEligibility = new("housing.filterEligibility", "Who can buy");
        public static readonly LocString FilterDivision = new("housing.filterDivision", "Division");
        public static readonly LocString FilterData = new("housing.filterData", "Data");
        public static readonly LocString FilterOtherPhases = new("housing.filterOtherPhases", "Other");
        public static readonly LocString FilterFreshOnly = new("housing.filterFreshOnly", "Fresh scans only");
        public static readonly LocString FilterWatchedOnly = new("housing.filterWatchedOnly", "Watched plots only");
        public static readonly LocString FilterMaxEntries = new("housing.filterMaxEntries", "Max reported entries");
        public static readonly LocString FilterAnyEntries = new("housing.filterAnyEntries", "Any");
        public static readonly LocString ShowAvailableOnly = new("housing.showAvailableOnly", "Available only");
        public static readonly LocString ShowAllPlots = new("housing.showAllPlots", "All plots");

        public static readonly LocString SortEntries = new("housing.sortEntries", "Fewest entries");
        public static readonly LocString SortScanned = new("housing.sortScanned", "Recently scanned");
        public static readonly LocString SortSize = new("housing.sortSize", "Plot size");
        public static readonly LocString SortPrice = new("housing.sortPrice", "Price");
        public static readonly LocString SortWard = new("housing.sortWard", "Ward and plot");

        public static readonly LocString LoadingFirst =
            new("housing.loadingFirst", "Checking residential listings…");
        public static readonly LocString NoFilterMatches =
            new("housing.noFilterMatches", "No plots match the current filters.");
        public static readonly LocString NoOpenings =
            new("housing.noOpenings", "No available plots were reported in Ward {0}.");
        public static readonly LocString NoScans =
            new("housing.noScans", "No recent housing scans are available for this ward.");
        public static readonly LocString NoScansHint = new("housing.noScansHint",
            "A missing report does not mean every plot is sold: it means nobody has walked this ward recently.");
        public static readonly LocString Offline = new("housing.offline", "Placard data could not be reached.");
        public static readonly LocString OfflineHint = new("housing.offlineHint",
            "Check your connection, or preview the demo data to explore the app offline.");
        public static readonly LocString NoWorldTitle = new("housing.noWorldTitle", "Pick a world to start");
        public static readonly LocString NoWorldHint = new("housing.noWorldHint",
            "Placard could not read your home world yet. Choose one and it becomes your preferred world.");
        public static readonly LocString WatchlistEmpty = new("housing.watchlistEmpty", "No watched plots yet");
        public static readonly LocString WatchlistEmptyHint = new("housing.watchlistEmptyHint",
            "Tap a plot on the map and choose Watch to keep an eye on it here.");
        public static readonly LocString NoLongerReported =
            new("housing.noLongerReported", "No longer reported, last seen {0}");
        public static readonly LocString LastKnown = new("housing.lastKnown", "Last known state");
        public static readonly LocString ClearWatchlist = new("housing.clearWatchlist", "Clear watchlist");
        public static readonly LocString ClearWatchlistConfirm = new("housing.clearWatchlistConfirm",
            "Remove all {0} watched plots? Their reminders are cancelled too.");
        public static readonly LocString MapHint = new("housing.mapHint",
            "Available plots appear as markers. Select a marker to view its lottery details.");
        public static readonly LocString GotIt = new("housing.gotIt", "Got it");

        public static readonly LocString LegendSmall = new("housing.legendSmall", "Circle: small");
        public static readonly LocString LegendMedium = new("housing.legendMedium", "Diamond: medium");
        public static readonly LocString LegendLarge = new("housing.legendLarge", "Hexagon: large");
        public static readonly LocString LegendWatched = new("housing.legendWatched", "Notch: watched");
        public static readonly LocString LegendStale = new("housing.legendStale", "Dashed ring: stale scan");
        public static readonly LocString LegendSelected = new("housing.legendSelected", "Outer ring: selected");

        public static readonly LocString SettingsData = new("housing.settingsData", "Data");
        public static readonly LocString SettingsWorld = new("housing.settingsWorld", "World");
        public static readonly LocString SettingsNotifications = new("housing.settingsNotifications", "Reminders");
        public static readonly LocString SettingsMap = new("housing.settingsMap", "Map");
        public static readonly LocString SettingsDiagnostics = new("housing.settingsDiagnostics", "Diagnostics");
        public static readonly LocString AutoRefresh = new("housing.autoRefresh", "Refresh automatically");
        public static readonly LocString RefreshInterval = new("housing.refreshInterval", "Refresh every");
        public static readonly LocString RefreshMinutes = new("housing.refreshMinutes", "{0} min");
        public static readonly LocString FollowCurrentWorld =
            new("housing.followCurrentWorld", "Follow the world I am visiting");
        public static readonly LocString FollowCurrentWorldHint = new("housing.followCurrentWorldHint",
            "Off by default: your preferred world stays put when you world-visit.");
        public static readonly LocString PreferredWorld = new("housing.preferredWorld", "Preferred world");
        public static readonly LocString NotifyEntry = new("housing.notifyEntry", "Entry period reminders");
        public static readonly LocString NotifyResults = new("housing.notifyResults", "Results period reminders");
        public static readonly LocString ReminderLead = new("housing.reminderLead", "Default lead time");
        public static readonly LocString FreshnessThreshold = new("housing.freshnessThreshold", "Treat scans as live for");
        public static readonly LocString ClearCache = new("housing.clearCache", "Clear saved housing data");
        public static readonly LocString MinutesSuffix = new("housing.minutesSuffix", "min");
        public static readonly LocString ReminderLeadHint = new("housing.reminderLeadHint",
            "Used when you create a reminder. You can still pick a different lead time per plot.");
        public static readonly LocString GameMapHint = new("housing.gameMapHint",
            "The district map and plot positions are read from your own game installation.");
        public static readonly LocString GameMapUnavailable =
            new("housing.gameMapUnavailable", "District map unavailable");
        public static readonly LocString GameMapUnavailableHint = new("housing.gameMapUnavailableHint",
            "Placard could not read this district's map from your game files, so it has no plot positions to draw. The list shows the same plots without a map.");
        public static readonly LocString GameMapUnavailableDetail = new("housing.gameMapUnavailableDetail",
            "District map unavailable ({0}). Copy the map diagnostics below to see why.");
        public static readonly LocString CopyMapDiagnostics =
            new("housing.copyMapDiagnostics", "Copy map diagnostics");
        public static readonly LocString CopiedMapDiagnostics = new("housing.copiedMapDiagnostics",
            "Map diagnostics copied to the clipboard.");
        public static readonly LocString MapSourceLabel = new("housing.mapSourceLabel", "Map source");
        public static readonly LocString ProviderStatus = new("housing.providerStatus", "Provider");
        public static readonly LocString LastRefresh = new("housing.lastRefresh", "Last successful refresh");
        public static readonly LocString OpenPlotsReported = new("housing.openPlotsReported", "Reported openings");
        public static readonly LocString ProxyCacheAge = new("housing.proxyCacheAge", "Service cache age");
        public static readonly LocString DataSourceNotice = new("housing.dataSourceNotice",
            "Placard reads its own service, which polls and caches the public PaissaDB API once for all users rather than each client polling it. The PaissaHouse plugin is not required.");
        public static readonly LocString RefreshIntervalHint = new("housing.refreshIntervalHint",
            "Placard polls no faster than every {0} minutes, and only while this app is open. Ward data only changes when a player walks the ward, so checking more often shows you nothing new. Refresh manually any time.");

        public static readonly LocString NotifyEntryTitle = new("housing.notifyEntryTitle", "Housing Reminder");
        public static readonly LocString NotifyEntryBody =
            new("housing.notifyEntryBody", "Plot {0} in {1} has {2} left in the entry period.");
        public static readonly LocString NotifyEntryDetail =
            new("housing.notifyEntryDetail", "Reported entries: {0} · {1}");
        public static readonly LocString NotifyResultsTitle = new("housing.notifyResultsTitle", "Housing Results");
        public static readonly LocString NotifyResultsBody = new("housing.notifyResultsBody",
            "The results period ends in {0}. Check the estate placard before the claim or refund window closes.");
    }
}
