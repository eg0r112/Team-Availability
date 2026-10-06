using TeamAvailability.Domain.Common;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Domain.Entities;

public sealed class TeamMember : AuditableEntity
{
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public TeamRole Role { get; private set; } = TeamRole.Member;
    public string TimeZoneId { get; private set; } = "UTC";

    public ICollection<Absence> Absences { get; private set; } = [];
    public MemberPresence? Presence { get; private set; }

    private TeamMember() { }

    public static TeamMember Create(Guid teamId, Guid userId, string timeZoneId, TeamRole role = TeamRole.Member)
    {
        return new TeamMember
        {
            TeamId = teamId,
            UserId = userId,
            TimeZoneId = timeZoneId,
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
