using MediatR;
using TeamAvailability.Application.Common.Behaviors;
using TeamAvailability.Contracts.OnCall;

namespace TeamAvailability.Application.Features.OnCall.CreateShift;

public sealed record CreateOnCallShiftCommand(
    Guid TeamId,
    Guid TeamMemberId,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc) : IRequest<OnCallShiftResponse>, ITransactionalRequest;
