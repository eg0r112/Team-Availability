using TeamAvailability.Domain.Common;

namespace TeamAvailability.Domain.Entities;

public sealed class IdempotencyRecord : BaseEntity
{
    public string Key { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public string ResponseBody { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }

    private IdempotencyRecord() { }

    public static IdempotencyRecord Create(string key, string requestHash, int statusCode, string responseBody, TimeSpan ttl)
    {
        var now = DateTimeOffset.UtcNow;
        return new IdempotencyRecord
        {
            Key = key,
            RequestHash = requestHash,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            CreatedAt = now,
            ExpiresAt = now.Add(ttl)
        };
    }
}
