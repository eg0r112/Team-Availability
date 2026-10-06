using MediatR;
using TeamAvailability.Contracts.Availability;

namespace TeamAvailability.Application.Features.Availability.GetTeamAvailability;

public sealed record GetTeamAvailabilityQuery(Guid TeamId, DateOnly? Date) : IRequest<TeamAvailabilityResponse>;
