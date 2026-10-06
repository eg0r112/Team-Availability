using TeamAvailability.Domain.Common;

namespace TeamAvailability.Domain.Entities;

public sealed class AuditEntry : BaseEntity
{
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string? ChangesJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? CorrelationId { get; private set; }

    private AuditEntry() { }

    public static AuditEntry Create(
        string entityName,
        Guid entityId,
        string action,
        Guid? userId,
        string? changesJson,
        string? correlationId)
    {
        return new AuditEntry
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            ChangesJson = changesJson,
            CorrelationId = correlationId,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }
}
