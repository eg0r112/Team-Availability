using MediatR;
using TeamAvailability.Application.Common.Behaviors;
using TeamAvailability.Contracts.Teams;

namespace TeamAvailability.Application.Features.Teams.CreateTeam;

public sealed record CreateTeamCommand(string Name, string? Description) : IRequest<TeamResponse>, ITransactionalRequest;
