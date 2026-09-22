using System.Globalization;
using Dalamud.Plugin.Ipc;

using HousingAddress = (string Name, int World, int City, int Ward, int PropertyType, int Plot, int Apartment,
    bool ApartmentSubdivision, bool AliasEnabled, string Alias);

namespace Placard.Core.Venues;

internal enum LifestreamOutcome
{
    Started,
    NotInstalled,
    Busy,
    NotAttuned,
    CannotTeleportNow,
}

internal static class LifestreamBridge
{
    private const string InternalName = "Lifestream";
    private const int HouseProperty = 0;
    private const int ApartmentWing = 1;

    private static ICallGateSubscriber<bool>? busyGate;
    private static ICallGateSubscriber<HousingAddress, object>? housingAddressGate;

    public static bool IsAvailable()
    {
        foreach (var plugin in Plugin.PluginInterface.InstalledPlugins)
        {
            if (string.Equals(plugin.InternalName, InternalName, StringComparison.Ordinal) && plugin.IsLoaded)
            {
                return true;
            }
        }

        return false;
    }

    public static LifestreamOutcome TravelToHousingPlot(uint worldId, uint cityAetheryteRowId, int ward, int plot)
    {
        if (!IsAvailable())
        {
            return LifestreamOutcome.NotInstalled;
        }

        if (IsBusy())
        {
            return LifestreamOutcome.Busy;
        }

        if (!IsAttuned(cityAetheryteRowId))
        {
            return LifestreamOutcome.NotAttuned;
        }

        return GoToHousingAddress(worldId, cityAetheryteRowId, ward, plot)
            ? LifestreamOutcome.Started
            : LifestreamOutcome.CannotTeleportNow;
    }

    public static string HousingCommand(string worldName, string districtName, int ward, int plot) =>
        string.Create(CultureInfo.InvariantCulture, $"/li {worldName}, {districtName}, W{ward}, P{plot}");

    public static bool IsBusy()
    {
        try
        {
            busyGate ??= Plugin.PluginInterface.GetIpcSubscriber<bool>($"{InternalName}.IsBusy");
            return busyGate.InvokeFunc();
        }
        catch (Exception exception)
        {
            PlacardLog.Warning(exception, "[Lifestream] IsBusy failed");
            return false;
        }
    }

    private static bool GoToHousingAddress(uint worldId, uint cityAetheryteRowId, int ward, int plot)
    {
        try
        {
            housingAddressGate ??= Plugin.PluginInterface.GetIpcSubscriber<HousingAddress, object>(
                $"{InternalName}.GoToHousingAddress");
            housingAddressGate.InvokeAction((string.Empty, (int)worldId, (int)cityAetheryteRowId, ward, HouseProperty,
                plot, ApartmentWing, false, false, string.Empty));
            return true;
        }
        catch (Exception exception)
        {
            PlacardLog.Warning(exception, "[Lifestream] GoToHousingAddress failed");
            return false;
        }
    }

    private static bool IsAttuned(uint aetheryteRowId)
    {
        if (aetheryteRowId == 0)
        {
            return false;
        }

        var attuned = Plugin.AetheryteList;
        for (var index = 0; index < attuned.Length; index++)
        {
            if (attuned[index] is { SubIndex: 0 } entry && entry.AetheryteId == aetheryteRowId)
            {
                return true;
            }
        }

        return false;
    }
}
