using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Authorization;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.Teams;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Enums;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Teams.AddMember;

public sealed class AddTeamMemberCommandHandler(
    IApplicationDbContext db,
    ITeamAuthorizationService teamAuthorization) : IRequestHandler<AddTeamMemberCommand, TeamMemberResponse>
{
    public async Task<TeamMemberResponse> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        await teamAuthorization.EnsureCanManageTeamAsync(request.TeamId, cancellationToken);

        var teamExists = await db.Teams.AnyAsync(t => t.Id == request.TeamId, cancellationToken);
        if (!teamExists)
            throw new NotFoundException("Team not found.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var alreadyMember = await db.TeamMembers
            .AnyAsync(m => m.TeamId == request.TeamId && m.UserId == request.UserId, cancellationToken);
        if (alreadyMember)
            throw new ConflictException("User is already a team member.");

        var role = Enum.Parse<TeamRole>(request.Role);
        var member = TeamMember.Create(request.TeamId, request.UserId, request.TimeZoneId, role);
        db.TeamMembers.Add(member);

        return new TeamMemberResponse(member.Id, user.Id, user.DisplayName, user.Email, member.TimeZoneId, member.Role.ToString());
    }
}
