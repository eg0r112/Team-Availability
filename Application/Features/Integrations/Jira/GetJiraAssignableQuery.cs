using MediatR;
using TeamAvailability.Contracts.Integrations;

namespace TeamAvailability.Application.Features.Integrations.Jira;

public sealed record GetJiraAssignableQuery(Guid TeamId, DateOnly? Date) : IRequest<JiraAssignableResponse>;
