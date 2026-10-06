using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Availability;

namespace TeamAvailability.Infrastructure.Caching;

public sealed class RedisAvailabilityCache(IDistributedCache cache) : IAvailabilityCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TeamAvailabilityResponse?> GetAsync(Guid teamId, DateOnly date, CancellationToken cancellationToken)
    {
        var bytes = await cache.GetAsync(BuildKey(teamId, date), cancellationToken);
        return bytes is null ? null : JsonSerializer.Deserialize<TeamAvailabilityResponse>(bytes, JsonOptions);
    }

    public async Task SetAsync(TeamAvailabilityResponse response, CancellationToken cancellationToken)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(response, JsonOptions);
        await cache.SetAsync(BuildKey(response.TeamId, response.Date), bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
        }, cancellationToken);
    }

    public Task InvalidateTeamAsync(Guid teamId, CancellationToken cancellationToken) =>
        // Pattern invalidation is not supported by IDistributedCache; background service recalculates proactively.
        Task.CompletedTask;

    private static string BuildKey(Guid teamId, DateOnly date) => $"availability:{teamId}:{date:yyyy-MM-dd}";
}
