using Placard.Core.Housing;

namespace Placard.Windows.Housing;

internal enum HousingRoute : byte
{
    Map,
    Watchlist,
    Details,
    Settings,
    WorldPicker,
}

internal enum HousingBrowseMode : byte
{
    Map,
    List,
}

internal sealed class HousingView
{
    public static readonly HousingView Root = new(HousingRoute.Map);

    public HousingView(HousingRoute route, HousingPlotKey plot = default)
    {
        Route = route;
        Plot = plot;
    }

    public HousingRoute Route { get; }
    public HousingPlotKey Plot { get; }
}
