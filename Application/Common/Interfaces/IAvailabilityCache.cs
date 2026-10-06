using TeamAvailability.Contracts.Availability;

namespace TeamAvailability.Application.Common.Interfaces;

public interface IAvailabilityCache
{
    Task<TeamAvailabilityResponse?> GetAsync(Guid teamId, DateOnly date, CancellationToken cancellationToken);
    Task SetAsync(TeamAvailabilityResponse response, CancellationToken cancellationToken);
    Task InvalidateTeamAsync(Guid teamId, CancellationToken cancellationToken);
}
