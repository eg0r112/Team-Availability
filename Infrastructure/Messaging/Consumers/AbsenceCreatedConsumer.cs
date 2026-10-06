using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Application.Events;
using TeamAvailability.Infrastructure.Persistence;

namespace TeamAvailability.Infrastructure.Messaging.Consumers;

public sealed class AbsenceCreatedConsumer(
    ApplicationDbContext db,
    ITelegramNotificationService telegram,
    IAvailabilityCache cache,
    ILogger<AbsenceCreatedConsumer> logger) : IConsumer<AbsenceCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AbsenceCreatedIntegrationEvent> context)
    {
        var message = context.Message;
        await cache.InvalidateTeamAsync(message.TeamId, context.CancellationToken);

        if (message.ReplacementMemberId.HasValue)
        {
            var replacement = await db.TeamMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == message.ReplacementMemberId, context.CancellationToken);

            if (replacement?.User.TelegramChatId is not null)
            {
                await telegram.SendMessageAsync(
                    replacement.User.TelegramChatId,
                    $"You are assigned as replacement for absence {message.StartDate:d} — {message.EndDate:d}.",
                    context.CancellationToken);
            }
        }

        logger.LogInformation("Processed AbsenceCreated event {AbsenceId}", message.AbsenceId);
    }
}
