using MediatR;
using TeamAvailability.Contracts.Teams;

namespace TeamAvailability.Application.Features.Teams.ListTeams;

public sealed record ListTeamsQuery : IRequest<IReadOnlyList<TeamSummaryResponse>>;
