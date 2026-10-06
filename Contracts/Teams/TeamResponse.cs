namespace TeamAvailability.Contracts.Teams;

public sealed record TeamResponse(Guid Id, string Name, string? Description, IReadOnlyList<TeamMemberResponse> Members);

public sealed record TeamMemberResponse(Guid Id, Guid UserId, string DisplayName, string Email, string TimeZoneId, string Role);
