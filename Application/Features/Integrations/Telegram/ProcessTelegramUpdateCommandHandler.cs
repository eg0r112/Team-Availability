using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Application.Features.Integrations.Telegram;

public sealed class ProcessTelegramUpdateCommandHandler(IApplicationDbContext db) : IRequestHandler<ProcessTelegramUpdateCommand, string>
{
    public async Task<string> Handle(ProcessTelegramUpdateCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.TeamMemberships)
            .ThenInclude(m => m.Presence)
            .FirstOrDefaultAsync(u => u.TelegramChatId == request.ChatId.ToString(), cancellationToken);

        if (user is null)
            return "Account not linked. Ask your team lead to link your Telegram chat ID.";

        var command = request.Text?.Trim().ToLowerInvariant() ?? string.Empty;

        return command switch
        {
            "/working" or "/work" => await SetPresence(user, PresenceStatus.Working, cancellationToken),
            "/away" => await SetPresence(user, PresenceStatus.Away, cancellationToken),
            "/status" => await GetStatus(user, cancellationToken),
            _ => "Commands: /working, /away, /status"
        };
    }

    private async Task<string> SetPresence(User user, PresenceStatus status, CancellationToken cancellationToken)
    {
        foreach (var member in user.TeamMemberships)
        {
            if (member.Presence is null)
            {
                var presence = MemberPresence.Create(member.Id, status);
                db.MemberPresences.Add(presence);
            }
            else
            {
                member.Presence.UpdateStatus(status);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return $"Status updated: {status}";
    }

    private async Task<string> GetStatus(User user, CancellationToken cancellationToken)
    {
        var member = user.TeamMemberships.FirstOrDefault();
        if (member is null)
            return "You are not assigned to any team.";

        var presence = member.Presence?.Status.ToString() ?? PresenceStatus.Away.ToString();
        var nextShift = await db.OnCallShifts
            .Where(s => s.TeamMemberId == member.Id && s.EndUtc > DateTimeOffset.UtcNow)
            .OrderBy(s => s.StartUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return nextShift is null
            ? $"Current status: {presence}. No upcoming on-call shifts."
            : $"Current status: {presence}. Next on-call: {nextShift.StartUtc:u} — {nextShift.EndUtc:u} UTC";
    }
}
