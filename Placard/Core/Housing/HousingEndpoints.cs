using System.Globalization;

namespace Placard.Core.Housing;

internal static class HousingEndpoints
{
    public const string BaseUrl = "https://housing-api.yozoracho.dev/api/v1";
    public const string DisplayName = "Placard housing service";

    public static string Worlds(string baseUrl) => string.Concat(Trim(baseUrl), "/worlds");

    public static string World(string baseUrl, uint worldId) =>
        string.Concat(Trim(baseUrl), "/worlds/", worldId.ToString(CultureInfo.InvariantCulture));

    private static string Trim(string baseUrl) => baseUrl.TrimEnd('/');
}
