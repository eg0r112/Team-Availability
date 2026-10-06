namespace TeamAvailability.Contracts.OnCall;

public sealed record CreateOnCallShiftRequest(Guid TeamMemberId, DateTimeOffset StartUtc, DateTimeOffset EndUtc);
