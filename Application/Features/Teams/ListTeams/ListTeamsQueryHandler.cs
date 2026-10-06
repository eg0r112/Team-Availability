using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Teams;

namespace TeamAvailability.Application.Features.Teams.ListTeams;

public sealed class ListTeamsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<ListTeamsQuery, IReadOnlyList<TeamSummaryResponse>>
{
    public async Task<IReadOnlyList<TeamSummaryResponse>> Handle(ListTeamsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Teams.AsNoTracking();

        if (!currentUser.IsAdmin)
        {
            var myTeamIds = await db.TeamMembers
                .Where(m => m.UserId == currentUser.UserId)
                .Select(m => m.TeamId)
                .ToListAsync(cancellationToken);

            query = query.Where(t => myTeamIds.Contains(t.Id));
        }

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new TeamSummaryResponse(t.Id, t.Name, t.Description, t.Members.Count))
            .ToListAsync(cancellationToken);
    }
}
