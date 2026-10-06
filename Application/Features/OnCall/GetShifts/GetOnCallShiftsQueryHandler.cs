using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Authorization;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.OnCall;

namespace TeamAvailability.Application.Features.OnCall.GetShifts;

public sealed class GetOnCallShiftsQueryHandler(
    IApplicationDbContext db,
    ITeamAuthorizationService teamAuthorization,
    IJiraIntegrationSettings jiraSettings) : IRequestHandler<GetOnCallShiftsQuery, IReadOnlyList<OnCallShiftResponse>>
{
    public async Task<IReadOnlyList<OnCallShiftResponse>> Handle(GetOnCallShiftsQuery request, CancellationToken cancellationToken)
    {
        await teamAuthorization.EnsureCanAccessTeamAsync(request.TeamId, cancellationToken);

        var shifts = await db.OnCallShifts
            .AsNoTracking()
            .Where(s => s.TeamId == request.TeamId)
            .Include(s => s.TeamMember)
            .ThenInclude(m => m.User)
            .OrderBy(s => s.StartUtc)
            .ToListAsync(cancellationToken);

        return shifts
            .Select(s => new OnCallShiftResponse(
                s.Id,
                s.TeamId,
                s.TeamMemberId,
                s.TeamMember.User.DisplayName,
                s.StartUtc,
                s.EndUtc,
                s.JiraIssueKey,
                s.JiraIssueKey is null || string.IsNullOrWhiteSpace(jiraSettings.BaseUrl)
                    ? null
                    : $"{jiraSettings.BaseUrl}/browse/{s.JiraIssueKey}"))
            .ToList();
    }
}
