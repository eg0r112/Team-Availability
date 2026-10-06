using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Infrastructure.Caching;

public sealed class RedisIdempotencyStore(IDistributedCache cache) : IIdempotencyStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IdempotencyResult?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await cache.GetAsync($"idempotency:{key}", cancellationToken);
        return bytes is null ? null : JsonSerializer.Deserialize<IdempotencyResult>(bytes, JsonOptions);
    }

    public async Task SaveAsync(string key, string requestHash, int statusCode, string responseBody, CancellationToken cancellationToken = default)
    {
        var result = new IdempotencyResult(statusCode, responseBody, requestHash);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(result, JsonOptions);
        await cache.SetAsync($"idempotency:{key}", bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        }, cancellationToken);
    }
}
