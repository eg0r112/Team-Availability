using TeamAvailability.Domain.Common;

namespace TeamAvailability.Domain.Entities;

public sealed class Team : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<TeamMember> Members { get; private set; } = [];
    public ICollection<OnCallShift> OnCallShifts { get; private set; } = [];

    private Team() { }

    public static Team Create(string name, string? description = null)
    {
        return new Team
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
