using MediatR;
using TeamAvailability.Application.Common.Behaviors;

namespace TeamAvailability.Application.Features.Users.LinkTelegram;

public sealed record LinkTelegramCommand(string ChatId) : IRequest<Unit>, ITransactionalRequest;
