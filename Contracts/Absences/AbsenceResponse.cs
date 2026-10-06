namespace TeamAvailability.Contracts.Absences;

public sealed record AbsenceResponse(
    Guid Id,
    Guid TeamMemberId,
    string MemberName,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ReplacementMemberId,
    string? ReplacementMemberName,
    string? Notes);
