namespace TeamAvailability.Application.Events;

public sealed record AbsenceCreatedIntegrationEvent(
    Guid AbsenceId,
    Guid TeamId,
    Guid TeamMemberId,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ReplacementMemberId);
