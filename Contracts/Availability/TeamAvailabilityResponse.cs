namespace TeamAvailability.Contracts.Availability;

public sealed record TeamAvailabilityResponse(
    Guid TeamId,
    DateOnly Date,
    IReadOnlyList<MemberAvailabilityDto> Members);

public sealed record MemberAvailabilityDto(
    Guid TeamMemberId,
    Guid UserId,
    string DisplayName,
    string TimeZoneId,
    bool IsAvailable,
    string? UnavailableReason,
    string? ReplacementName,
    string PresenceStatus);
