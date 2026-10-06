namespace TeamAvailability.Application.Common.Interfaces;

public interface IIdempotencyStore
{
    Task<IdempotencyResult?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SaveAsync(string key, string requestHash, int statusCode, string responseBody, CancellationToken cancellationToken = default);
}

public sealed record IdempotencyResult(int StatusCode, string ResponseBody, string RequestHash);
