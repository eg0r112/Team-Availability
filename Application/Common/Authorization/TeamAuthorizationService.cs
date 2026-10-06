using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Domain.Enums;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Common.Authorization;

public interface ITeamAuthorizationService
{
    Task EnsureCanAccessTeamAsync(Guid teamId, CancellationToken cancellationToken);
    Task EnsureCanManageTeamAsync(Guid teamId, CancellationToken cancellationToken);
}

public sealed class TeamAuthorizationService(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : ITeamAuthorizationService
{
    public async Task EnsureCanAccessTeamAsync(Guid teamId, CancellationToken cancellationToken)
    {
        if (currentUser.IsAdmin)
            return;

        var hasAccess = await db.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == currentUser.UserId, cancellationToken);

        if (!hasAccess)
            throw new NotFoundException("Team not found or access denied.");
    }

    public async Task EnsureCanManageTeamAsync(Guid teamId, CancellationToken cancellationToken)
    {
        if (currentUser.IsAdmin)
            return;

        var isLead = await db.TeamMembers
            .AnyAsync(m => m.TeamId == teamId && m.UserId == currentUser.UserId && m.Role == TeamRole.TeamLead, cancellationToken);

        if (!isLead)
            throw new DomainException("Only team leads or admins can perform this action.");
    }
}
