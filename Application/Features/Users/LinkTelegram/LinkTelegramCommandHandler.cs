using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Users.LinkTelegram;

public sealed class LinkTelegramCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser) : IRequestHandler<LinkTelegramCommand, Unit>
{
    public async Task<Unit> Handle(LinkTelegramCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            throw new DomainException("Not authenticated.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.LinkTelegram(request.ChatId.Trim());
        return Unit.Value;
    }
}
