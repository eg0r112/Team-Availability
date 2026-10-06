using MediatR;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Teams;
using TeamAvailability.Domain.Entities;

namespace TeamAvailability.Application.Features.Teams.CreateTeam;

public sealed class CreateTeamCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateTeamCommand, TeamResponse>
{
    public Task<TeamResponse> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var team = Team.Create(request.Name, request.Description);
        db.Teams.Add(team);

        return Task.FromResult(new TeamResponse(team.Id, team.Name, team.Description, []));
    }
}
