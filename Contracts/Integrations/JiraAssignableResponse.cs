namespace TeamAvailability.Contracts.Integrations;

public sealed record JiraAssignableResponse(
    Guid TeamId,
    DateOnly Date,
    IReadOnlyList<JiraMemberDto> Available,
    IReadOnlyList<JiraMemberDto> Unavailable);

public sealed record JiraMemberDto(
    Guid TeamMemberId,
    string DisplayName,
    string PresenceStatus,
    string TimeZoneId,
    string? Reason,
    string? Replacement);
