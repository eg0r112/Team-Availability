namespace TeamAvailability.Contracts.Absences;

public sealed record CreateAbsenceRequest(
    Guid TeamMemberId,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ReplacementMemberId,
    string? Notes);
