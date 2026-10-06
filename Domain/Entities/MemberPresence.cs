using TeamAvailability.Domain.Common;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Domain.Entities;

public sealed class MemberPresence : BaseEntity
{
    public Guid TeamMemberId { get; private set; }
    public TeamMember TeamMember { get; private set; } = null!;
    public PresenceStatus Status { get; private set; } = PresenceStatus.Away;
    public DateTimeOffset LastChangedAt { get; private set; } = DateTimeOffset.UtcNow;

    private MemberPresence() { }

    public static MemberPresence Create(Guid teamMemberId, PresenceStatus status)
    {
        return new MemberPresence
        {
            TeamMemberId = teamMemberId,
            Status = status,
            LastChangedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateStatus(PresenceStatus status)
    {
        Status = status;
        LastChangedAt = DateTimeOffset.UtcNow;
    }
}
