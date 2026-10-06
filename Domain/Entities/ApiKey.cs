using TeamAvailability.Domain.Common;

namespace TeamAvailability.Domain.Entities;

public sealed class ApiKey : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string KeyHash { get; private set; } = string.Empty;
    public string KeyPrefix { get; private set; } = string.Empty;
    public Guid? TeamId { get; private set; }
    public Team? Team { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? ExpiresAt { get; private set; }

    private ApiKey() { }

    public static ApiKey Create(string name, string keyHash, string keyPrefix, Guid? teamId, DateTimeOffset? expiresAt)
    {
        return new ApiKey
        {
            Name = name,
            KeyHash = keyHash,
            KeyPrefix = keyPrefix,
            TeamId = teamId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
}
