using TeamAvailability.Domain.Common;
using TeamAvailability.Domain.Enums;
using TeamAvailability.Domain.Exceptions;
using TeamAvailability.Domain.Rules;

namespace TeamAvailability.Domain.Entities;

public sealed class Absence : AuditableEntity, ISoftDeletable
{
    public Guid TeamMemberId { get; private set; }
    public TeamMember TeamMember { get; private set; } = null!;
    public AbsenceType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public Guid? ReplacementMemberId { get; private set; }
    public TeamMember? ReplacementMember { get; private set; }
    public string? Notes { get; private set; }
    public byte[] RowVersion { get; private set; } = [];
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    private Absence() { }

    public static Absence Create(
        Guid teamMemberId,
        AbsenceType type,
        DateOnly startDate,
        DateOnly endDate,
        Guid? replacementMemberId,
        string? notes)
    {
        if (endDate < startDate)
            throw new DomainException("End date cannot be before start date.");

        if (type == AbsenceType.Vacation && replacementMemberId is null)
            throw new DomainException("Vacation requires a replacement team member.");

        if (replacementMemberId == teamMemberId)
            throw new DomainException("A member cannot be their own replacement.");

        return new Absence
        {
            TeamMemberId = teamMemberId,
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            ReplacementMemberId = replacementMemberId,
            Notes = notes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public bool Overlaps(DateOnly start, DateOnly end) =>
        AbsenceOverlapRules.Overlaps(StartDate, EndDate, start, end);

    public void SoftDelete(Guid deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }
}
