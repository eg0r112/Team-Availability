using MediatR;
using TeamAvailability.Application.Features.Availability.GetTeamAvailability;
using TeamAvailability.Contracts.Integrations;

namespace TeamAvailability.Application.Features.Integrations.Jira;

public sealed class GetJiraAssignableQueryHandler(IMediator mediator) : IRequestHandler<GetJiraAssignableQuery, JiraAssignableResponse>
{
    public async Task<JiraAssignableResponse> Handle(GetJiraAssignableQuery request, CancellationToken cancellationToken)
    {
        var availability = await mediator.Send(new GetTeamAvailabilityQuery(request.TeamId, request.Date), cancellationToken);

        var available = availability.Members
            .Where(m => m.IsAvailable)
            .Select(m => new JiraMemberDto(m.TeamMemberId, m.DisplayName, m.PresenceStatus, m.TimeZoneId, null, null))
            .ToList();

        var unavailable = availability.Members
            .Where(m => !m.IsAvailable)
            .Select(m => new JiraMemberDto(
                m.TeamMemberId,
                m.DisplayName,
                m.PresenceStatus,
                m.TimeZoneId,
                m.UnavailableReason,
                m.ReplacementName))
            .ToList();

        return new JiraAssignableResponse(availability.TeamId, availability.Date, available, unavailable);
    }
}
