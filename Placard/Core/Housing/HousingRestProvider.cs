using Placard.Core.Net;

namespace Placard.Core.Housing;

internal sealed class HousingRestProvider
{
    private const int PhaseEntry = 1;
    private const int PhaseResults = 2;
    private const int PhaseUnavailable = 3;
    private const int FlagLottery = 1;
    private const int FlagFreeCompany = 2;
    private const int FlagIndividual = 4;

    private readonly HttpService http;
    private readonly RequestThrottle throttle;
    private readonly Func<string> baseUrl;

    public HousingRestProvider(HttpService http, HousingProviderKind kind, string displayName, Func<string> baseUrl)
    {
        this.http = http;
        this.baseUrl = baseUrl;
        Kind = kind;
        DisplayName = displayName;
        throttle = new RequestThrottle(2, TimeSpan.FromMilliseconds(250));
    }

    public HousingProviderKind Kind { get; }

    public string DisplayName { get; }

    public int LastStatusCode { get; private set; }

    public int? LastProxyCacheAge { get; private set; }

    public string BaseUrl => baseUrl();

    public async Task<IReadOnlyList<HousingWorld>?> GetWorldsAsync(CancellationToken token)
    {
        using (await throttle.EnterAsync(token).ConfigureAwait(false))
        {
            var status = 0;
            var payload = await http.GetJsonAsync(HousingEndpoints.Worlds(BaseUrl),
                HousingJsonContext.Default.PaissaWorldSummaryArray, token, code => status = code)
                .ConfigureAwait(false);
            LastStatusCode = status;
            if (payload is null)
            {
                return null;
            }

            var worlds = new List<HousingWorld>(payload.Length);
            for (var index = 0; index < payload.Length; index++)
            {
                var entry = payload[index];
                if (entry.Id == 0 || string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                var dataCenter = entry.DataCenterName ?? string.Empty;
                worlds.Add(new HousingWorld(entry.Id, entry.Name, entry.DataCenterId, dataCenter,
                    HousingRegions.For(dataCenter)));
            }

            return worlds;
        }
    }

    public async Task<IReadOnlyList<HousingDistrictSnapshot>?> GetWorldAsync(uint worldId, CancellationToken token)
    {
        if (worldId == 0)
        {
            return null;
        }

        using (await throttle.EnterAsync(token).ConfigureAwait(false))
        {
            var status = 0;
            var payload = await http.GetJsonAsync(HousingEndpoints.World(BaseUrl, worldId),
                HousingJsonContext.Default.PaissaWorldDetail, token, code => status = code)
                .ConfigureAwait(false);
            LastStatusCode = status;
            if (payload is null)
            {
                return null;
            }

            var districts = payload.Districts ?? Array.Empty<PaissaDistrictDetail>();
            var worldName = payload.Name ?? string.Empty;
            var mapped = new List<HousingDistrictSnapshot>(HousingDistricts.All.Count);
            for (var index = 0; index < districts.Length; index++)
            {
                var district = districts[index];
                if (!HousingDistricts.TryGet(district.Id, out _))
                {
                    continue;
                }

                mapped.Add(Map(worldId, worldName, district.Id, district, payload.Proxy));
            }

            LastProxyCacheAge = ProxyAge(payload, districts);
            return mapped.Count == 0 ? null : mapped;
        }
    }

    private static int? ProxyAge(PaissaWorldDetail payload, PaissaDistrictDetail[] districts)
    {
        if (payload.Proxy?.CacheAgeSeconds is { } worldAge)
        {
            return worldAge;
        }

        for (var index = 0; index < districts.Length; index++)
        {
            if (districts[index].Proxy?.CacheAgeSeconds is { } districtAge)
            {
                return districtAge;
            }
        }

        return null;
    }

    private HousingDistrictSnapshot Map(uint worldId, string worldName, uint districtId,
        PaissaDistrictDetail payload, PaissaProxyInfo? worldProxy)
    {
        var raw = payload.OpenPlots ?? Array.Empty<PaissaOpenPlot>();
        var plots = new List<HousingPlot>(raw.Length);
        for (var index = 0; index < raw.Length; index++)
        {
            if (TryMapPlot(worldId, districtId, raw[index], out var plot))
            {
                plots.Add(plot);
            }
        }

        plots.Sort(HousingPlotOrder.ByDistrictWardThenPlot);

        var fetched = DateTime.UtcNow;
        if ((payload.Proxy?.CacheAgeSeconds ?? worldProxy?.CacheAgeSeconds) is { } age && age > 0)
        {
            fetched = fetched.AddSeconds(-age);
        }

        return new HousingDistrictSnapshot
        {
            WorldId = worldId,
            DistrictId = districtId,
            WorldName = worldName,
            DistrictName = string.IsNullOrEmpty(payload.Name) ? HousingDistricts.Name(districtId) : payload.Name,
            FetchedUtc = fetched,
            Source = Kind,
            Plots = plots,
        };
    }

    private static bool TryMapPlot(uint worldId, uint districtId, PaissaOpenPlot entry, out HousingPlot plot)
    {
        plot = null!;
        var ward = NormalizeWard(entry.WardNumber);
        var plotNumber = NormalizePlot(entry.PlotNumber);
        if (ward <= 0 || plotNumber <= 0)
        {
            return false;
        }

        plot = new HousingPlot
        {
            Key = new HousingPlotKey(worldId, districtId, ward, plotNumber),
            Size = MapSize(entry.Size),
            Price = entry.Price > 0 ? entry.Price : 0L,
            Eligibility = MapEligibility(entry.PurchaseSystem),
            Mode = MapMode(entry.PurchaseSystem),
            Phase = MapPhase(entry.LottoPhase),
            PhaseEndsUtc = FromUnixSeconds(entry.LottoPhaseUntil),
            Entries = entry.LottoEntries,
            LastSeenUtc = FromUnixSeconds(entry.LastUpdatedTime) ?? default,
            FirstSeenUtc = FromUnixSeconds(entry.FirstSeenTime) ?? default,
        };
        return true;
    }

    public static int NormalizeWard(int apiWardNumber) =>
        apiWardNumber is < 0 or >= HousingDistricts.DefaultWards ? 0 : apiWardNumber + 1;

    public static int NormalizePlot(int apiPlotNumber) =>
        apiPlotNumber is < 0 or >= HousingDistricts.PlotsPerWard ? 0 : apiPlotNumber + 1;

    public static HousingPlotSize MapSize(int apiSize) => apiSize switch
    {
        0 => HousingPlotSize.Small,
        1 => HousingPlotSize.Medium,
        2 => HousingPlotSize.Large,
        _ => HousingPlotSize.Unknown,
    };

    public static HousingLotteryPhase MapPhase(int? apiPhase) => apiPhase switch
    {
        PhaseEntry => HousingLotteryPhase.Entry,
        PhaseResults => HousingLotteryPhase.Results,
        PhaseUnavailable => HousingLotteryPhase.Unavailable,
        _ => HousingLotteryPhase.Unknown,
    };

    public static HousingPurchaseMode MapMode(int purchaseSystem)
    {
        if (purchaseSystem <= 0)
        {
            return HousingPurchaseMode.Unknown;
        }

        return (purchaseSystem & FlagLottery) != 0
            ? HousingPurchaseMode.Lottery
            : HousingPurchaseMode.FirstComeFirstServed;
    }

    public static HousingPurchaseEligibility MapEligibility(int purchaseSystem)
    {
        var freeCompany = (purchaseSystem & FlagFreeCompany) != 0;
        var individual = (purchaseSystem & FlagIndividual) != 0;
        if (freeCompany && individual)
        {
            return HousingPurchaseEligibility.Both;
        }

        if (freeCompany)
        {
            return HousingPurchaseEligibility.FreeCompany;
        }

        return individual ? HousingPurchaseEligibility.Private : HousingPurchaseEligibility.Unknown;
    }

    private static DateTime? FromUnixSeconds(double seconds) =>
        seconds <= 0d ? null : DateTimeOffset.FromUnixTimeMilliseconds((long)(seconds * 1000d)).UtcDateTime;

    private static DateTime? FromUnixSeconds(long? seconds) =>
        seconds is null or <= 0L ? null : DateTimeOffset.FromUnixTimeSeconds(seconds.Value).UtcDateTime;

    public void Dispose() => throttle.Dispose();
}

internal static class HousingPlotOrder
{
    public static readonly Comparison<HousingPlot> ByDistrictWardThenPlot = static (first, second) =>
    {
        var district = DistrictRank(first.Key.DistrictId).CompareTo(DistrictRank(second.Key.DistrictId));
        if (district != 0)
        {
            return district;
        }

        var ward = first.Key.Ward.CompareTo(second.Key.Ward);
        return ward != 0 ? ward : first.Key.Plot.CompareTo(second.Key.Plot);
    };

    private static int DistrictRank(uint districtId)
    {
        var districts = HousingDistricts.All;
        for (var index = 0; index < districts.Count; index++)
        {
            if (districts[index].Id == districtId)
            {
                return index;
            }
        }

        return districts.Count;
    }
}
