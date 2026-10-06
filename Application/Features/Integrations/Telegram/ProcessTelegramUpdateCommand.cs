using MediatR;
using TeamAvailability.Application.Common.Behaviors;

namespace TeamAvailability.Application.Features.Integrations.Telegram;

public sealed record ProcessTelegramUpdateCommand(long ChatId, long UserId, string? Text) : IRequest<string>, ITransactionalRequest;
