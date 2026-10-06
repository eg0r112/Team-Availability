using MediatR;
using TeamAvailability.Contracts.Teams;

namespace TeamAvailability.Application.Features.Teams.GetTeam;

public sealed record GetTeamQuery(Guid TeamId) : IRequest<TeamResponse>;
