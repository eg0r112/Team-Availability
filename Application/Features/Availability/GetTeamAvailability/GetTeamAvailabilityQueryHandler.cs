using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Authorization;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Application.Common.Services;
using TeamAvailability.Contracts.Availability;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Availability.GetTeamAvailability;

public sealed class GetTeamAvailabilityQueryHandler(
    IApplicationDbContext db,
    ITeamAuthorizationService teamAuthorization,
    IAvailabilityCache cache,
    IDateTimeProvider dateTimeProvider) : IRequestHandler<GetTeamAvailabilityQuery, TeamAvailabilityResponse>
{
    public async Task<TeamAvailabilityResponse> Handle(GetTeamAvailabilityQuery request, CancellationToken cancellationToken)
    {
        await teamAuthorization.EnsureCanAccessTeamAsync(request.TeamId, cancellationToken);

        var date = request.Date ?? DateOnly.FromDateTime(dateTimeProvider.UtcNow.UtcDateTime);

        var cached = await cache.GetAsync(request.TeamId, date, cancellationToken);
        if (cached is not null)
            return cached;

        var members = await db.TeamMembers
            .AsNoTracking()
            .Where(m => m.TeamId == request.TeamId)
            .Include(m => m.User)
            .Include(m => m.Presence)
            .Include(m => m.Absences.Where(a => !a.IsDeleted))
            .ThenInclude(a => a.ReplacementMember!)
            .ThenInclude(r => r.User)
            .ToListAsync(cancellationToken);

        if (members.Count == 0)
            throw new NotFoundException("Team not found or has no members.");

        var response = AvailabilityCalculator.Calculate(request.TeamId, date, members);
        await cache.SetAsync(response, cancellationToken);
        return response;
    }
}
