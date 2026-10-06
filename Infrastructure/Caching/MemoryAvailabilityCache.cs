using System.Collections.Concurrent;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Availability;

namespace TeamAvailability.Infrastructure.Caching;

public sealed class MemoryAvailabilityCache : IAvailabilityCache
{
    private readonly ConcurrentDictionary<string, TeamAvailabilityResponse> _cache = new();

    public Task<TeamAvailabilityResponse?> GetAsync(Guid teamId, DateOnly date, CancellationToken cancellationToken)
    {
        _cache.TryGetValue(Key(teamId, date), out var value);
        return Task.FromResult(value);
    }

    public Task SetAsync(TeamAvailabilityResponse response, CancellationToken cancellationToken)
    {
        _cache[Key(response.TeamId, response.Date)] = response;
        return Task.CompletedTask;
    }

    public Task InvalidateTeamAsync(Guid teamId, CancellationToken cancellationToken)
    {
        foreach (var key in _cache.Keys.Where(k => k.StartsWith($"{teamId}:")))
            _cache.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    private static string Key(Guid teamId, DateOnly date) => $"{teamId}:{date:yyyy-MM-dd}";
}
