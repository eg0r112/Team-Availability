namespace TeamAvailability.Contracts.Teams;

public sealed record TeamSummaryResponse(Guid Id, string Name, string? Description, int MemberCount);
