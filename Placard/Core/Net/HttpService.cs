using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Placard.Core.Net;

internal sealed class HttpService : IDisposable
{
    private const int CacheLimit = 64;

    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(20);

    private readonly HttpClient client;
    private readonly Dictionary<string, CacheEntry> etagCache = new(StringComparer.Ordinal);
    private readonly object sync = new();

    public HttpService(string userAgent)
    {
        client = new HttpClient(new SocketsHttpHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        });
        client.Timeout = Timeout.InfiniteTimeSpan;
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<T?> GetJsonAsync<T>(string url, JsonTypeInfo<T> typeInfo, CancellationToken token,
        Action<int>? onStatus = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var cached = Lookup(url);
        if (cached is not null)
        {
            request.Headers.TryAddWithoutValidation("If-None-Match", cached.ETag);
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
            timeout.CancelAfter(RequestTimeout);
            using var response = await client.SendAsync(request, timeout.Token).ConfigureAwait(false);
            onStatus?.Invoke((int)response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NotModified && cached is not null)
            {
                return JsonSerializer.Deserialize(cached.Body, typeInfo);
            }

            if (!response.IsSuccessStatusCode)
            {
                PlacardLog.Warning($"HTTP GET {url} returned {(int)response.StatusCode}");
                return default;
            }

            var body = await response.Content.ReadAsByteArrayAsync(timeout.Token).ConfigureAwait(false);
            Store(url, response.Headers.ETag?.Tag, body);
            return JsonSerializer.Deserialize(body, typeInfo);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return default;
        }
        catch (Exception exception)
        {
            PlacardLog.Warning(exception, $"HTTP GET {url} failed");
            onStatus?.Invoke(0);
            return default;
        }
    }

    public void Dispose()
    {
        client.Dispose();
    }

    private CacheEntry? Lookup(string url)
    {
        lock (sync)
        {
            return etagCache.TryGetValue(url, out var entry) ? entry : null;
        }
    }

    private void Store(string url, string? tag, byte[] body)
    {
        if (string.IsNullOrEmpty(tag))
        {
            return;
        }

        lock (sync)
        {
            if (etagCache.Count >= CacheLimit && !etagCache.ContainsKey(url))
            {
                etagCache.Clear();
            }

            etagCache[url] = new CacheEntry(tag, body);
        }
    }

    private sealed record CacheEntry(string ETag, byte[] Body);
}
