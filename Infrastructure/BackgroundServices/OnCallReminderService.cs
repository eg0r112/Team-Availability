using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Infrastructure.Persistence;

namespace TeamAvailability.Infrastructure.BackgroundServices;

public sealed class OnCallReminderService(
    IServiceScopeFactory scopeFactory,
    ILogger<OnCallReminderService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "On-call reminder dispatch failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task SendRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var telegram = scope.ServiceProvider.GetRequiredService<ITelegramNotificationService>();
        var jira = scope.ServiceProvider.GetRequiredService<IJiraClient>();

        if (!telegram.IsConfigured)
            return;

        var windowStart = DateTimeOffset.UtcNow;
        var windowEnd = windowStart.AddHours(24);

        var shifts = await db.OnCallShifts
            .Include(s => s.TeamMember)
            .ThenInclude(m => m.User)
            .Where(s => !s.ReminderSent && s.StartUtc >= windowStart && s.StartUtc <= windowEnd)
            .ToListAsync(cancellationToken);

        foreach (var shift in shifts)
        {
            var chatId = shift.TeamMember.User.TelegramChatId;
            if (string.IsNullOrWhiteSpace(chatId))
                continue;

            await telegram.SendMessageAsync(
                chatId,
                $"Reminder: you are on-call from {shift.StartUtc:u} to {shift.EndUtc:u} UTC.",
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(shift.JiraIssueKey) && jira.IsConfigured)
            {
                try
                {
                    await jira.AddCommentAsync(
                        shift.JiraIssueKey,
                        $"On-call reminder sent to {shift.TeamMember.User.DisplayName}. Shift: {shift.StartUtc:u} — {shift.EndUtc:u} UTC.",
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to add Jira reminder comment to {IssueKey}", shift.JiraIssueKey);
                }
            }

            shift.MarkReminderSent();
        }

        if (shifts.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Sent {Count} on-call reminders", shifts.Count);
    }
}
