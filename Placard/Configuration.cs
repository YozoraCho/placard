using Dalamud.Configuration;
using Placard.Core.Housing;

namespace Placard;

[Serializable]
internal sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public string Language { get; set; } = "en";

    public bool? Use24HourClock { get; set; }

    public uint HousingWorldId { get; set; }

    public uint HousingDistrictId { get; set; } = HousingDistricts.MistId;

    public int HousingWard { get; set; } = HousingDefaults.DefaultWard;

    public bool HousingFollowCurrentWorld { get; set; }

    public bool HousingAutoRefresh { get; set; } = true;

    public int HousingRefreshMinutes { get; set; } = HousingDefaults.RefreshMinutes;

    public int HousingLiveMinutes { get; set; } = 15;

    public int HousingRecentMinutes { get; set; } = 60;

    public bool HousingNotifyEntry { get; set; } = true;

    public bool HousingNotifyResults { get; set; } = true;

    public int HousingReminderMinutes { get; set; } = HousingDefaults.ReminderMinutes;

    public bool HousingFilterSmall { get; set; } = true;

    public bool HousingFilterMedium { get; set; } = true;

    public bool HousingFilterLarge { get; set; } = true;

    public bool HousingShowAllPlots { get; set; }

    public int HousingListSort { get; set; }

    public int HousingBrowseMode { get; set; }

    public bool HousingMapHintDismissed { get; set; }

    public List<HousingWatchRecord> HousingWatched { get; set; } = new();

    public List<HousingReminderRecord> HousingReminders { get; set; } = new();

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
