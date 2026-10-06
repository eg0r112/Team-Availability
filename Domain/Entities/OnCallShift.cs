using TeamAvailability.Domain.Common;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Domain.Entities;

public sealed class OnCallShift : AuditableEntity
{
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = null!;
    public Guid TeamMemberId { get; private set; }
    public TeamMember TeamMember { get; private set; } = null!;
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public bool ReminderSent { get; private set; }
    public string? JiraIssueKey { get; private set; }

    private OnCallShift() { }

    public static OnCallShift Create(Guid teamId, Guid teamMemberId, DateTimeOffset startUtc, DateTimeOffset endUtc)
    {
        if (endUtc <= startUtc)
            throw new DomainException("On-call shift end must be after start.");

        return new OnCallShift
        {
            TeamId = teamId,
            TeamMemberId = teamMemberId,
            StartUtc = startUtc,
            EndUtc = endUtc,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void MarkReminderSent() => ReminderSent = true;

    public void LinkJiraIssue(string issueKey) => JiraIssueKey = issueKey;
}
