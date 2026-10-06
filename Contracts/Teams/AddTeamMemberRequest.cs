namespace TeamAvailability.Contracts.Teams;

public sealed record AddTeamMemberRequest(Guid UserId, string TimeZoneId, string Role);
