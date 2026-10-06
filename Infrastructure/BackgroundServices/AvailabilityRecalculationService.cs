using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Application.Common.Services;
using TeamAvailability.Infrastructure.Persistence;

namespace TeamAvailability.Infrastructure.BackgroundServices;

public sealed class AvailabilityRecalculationService(
    IServiceScopeFactory scopeFactory,
    ILogger<AvailabilityRecalculationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RecalculateAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Availability recalculation failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task RecalculateAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<IAvailabilityCache>();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var teamIds = await db.Teams.AsNoTracking().Select(t => t.Id).ToListAsync(cancellationToken);

        foreach (var teamId in teamIds)
        {
            var members = await db.TeamMembers
                .AsNoTracking()
                .Where(m => m.TeamId == teamId)
                .Include(m => m.User)
                .Include(m => m.Presence)
                .Include(m => m.Absences.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.ReplacementMember!)
                .ThenInclude(r => r.User)
                .ToListAsync(cancellationToken);

            if (members.Count == 0)
                continue;

            var response = AvailabilityCalculator.Calculate(teamId, date, members);
            await cache.SetAsync(response, cancellationToken);
        }

        logger.LogInformation("Recalculated availability for {TeamCount} teams on {Date}", teamIds.Count, date);
    }
}
