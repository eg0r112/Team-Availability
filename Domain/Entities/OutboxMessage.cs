using TeamAvailability.Domain.Common;

namespace TeamAvailability.Domain.Entities;

public sealed class OutboxMessage : BaseEntity
{
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    public static OutboxMessage Create(string eventType, string payload)
    {
        return new OutboxMessage
        {
            EventType = eventType,
            Payload = payload,
            OccurredAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkProcessed() => ProcessedAt = DateTimeOffset.UtcNow;

    public void MarkFailed(string error) => Error = error;
}
