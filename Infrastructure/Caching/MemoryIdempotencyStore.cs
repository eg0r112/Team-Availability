using System.Collections.Concurrent;
using TeamAvailability.Application.Common.Interfaces;

namespace TeamAvailability.Infrastructure.Caching;

public sealed class MemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, IdempotencyResult> _store = new();

    public Task<IdempotencyResult?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(key, out var value);
        return Task.FromResult<IdempotencyResult?>(value);
    }

    public Task SaveAsync(string key, string requestHash, int statusCode, string responseBody, CancellationToken cancellationToken = default)
    {
        _store[key] = new IdempotencyResult(statusCode, responseBody, requestHash);
        return Task.CompletedTask;
    }
}
