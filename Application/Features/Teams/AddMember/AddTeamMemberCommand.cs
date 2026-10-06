using MediatR;
using TeamAvailability.Application.Common.Behaviors;
using TeamAvailability.Contracts.Teams;

namespace TeamAvailability.Application.Features.Teams.AddMember;

public sealed record AddTeamMemberCommand(Guid TeamId, Guid UserId, string TimeZoneId, string Role) : IRequest<TeamMemberResponse>, ITransactionalRequest;
