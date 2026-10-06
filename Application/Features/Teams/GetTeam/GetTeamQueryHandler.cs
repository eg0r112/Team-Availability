using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Authorization;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Teams;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Teams.GetTeam;

public sealed class GetTeamQueryHandler(
    IApplicationDbContext db,
    ITeamAuthorizationService teamAuthorization) : IRequestHandler<GetTeamQuery, TeamResponse>
{
    public async Task<TeamResponse> Handle(GetTeamQuery request, CancellationToken cancellationToken)
    {
        await teamAuthorization.EnsureCanAccessTeamAsync(request.TeamId, cancellationToken);

        var team = await db.Teams
            .AsNoTracking()
            .Include(t => t.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team not found.");

        var members = team.Members.Select(m => new TeamMemberResponse(
            m.Id, m.UserId, m.User.DisplayName, m.User.Email, m.TimeZoneId, m.Role.ToString())).ToList();

        return new TeamResponse(team.Id, team.Name, team.Description, members);
    }
}
