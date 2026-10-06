namespace TeamAvailability.Contracts.OnCall;

public sealed record OnCallShiftResponse(
    Guid Id,
    Guid TeamId,
    Guid TeamMemberId,
    string MemberName,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    string? JiraIssueKey = null,
    string? JiraIssueUrl = null);
