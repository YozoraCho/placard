using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace Placard.Core.Game;

internal sealed class GameData
{
    private readonly IDataManager data;
    private readonly IObjectTable objectTable;

    public GameData(IDataManager data, IObjectTable objectTable)
    {
        this.data = data;
        this.objectTable = objectTable;
    }

    public uint LocalHomeWorldId => objectTable.LocalPlayer?.HomeWorld.RowId ?? 0u;

    public uint LocalCurrentWorldId => objectTable.LocalPlayer?.CurrentWorld.RowId ?? 0u;

    public string WorldName(uint rowId)
    {
        if (rowId != 0 && data.GetExcelSheet<World>().TryGetRow(rowId, out var world))
        {
            return world.Name.ExtractText();
        }

        return string.Empty;
    }

    public string DataCenterName(uint worldId)
    {
        if (worldId != 0 && data.GetExcelSheet<World>().TryGetRow(worldId, out var world) &&
            world.DataCenter.RowId != 0)
        {
            return world.DataCenter.Value.Name.ExtractText();
        }

        return string.Empty;
    }

    public string RegionName(uint worldId)
    {
        if (worldId != 0 && data.GetExcelSheet<World>().TryGetRow(worldId, out var world) &&
            world.DataCenter.RowId != 0)
        {
            return RegionNameFromId(world.DataCenter.Value.Region.RowId);
        }

        return string.Empty;
    }

    private static string RegionNameFromId(uint region) =>
        region switch
        {
            1 => "Japan",
            2 => "North-America",
            3 => "Europe",
            4 => "Oceania",
            _ => string.Empty,
        };
}
