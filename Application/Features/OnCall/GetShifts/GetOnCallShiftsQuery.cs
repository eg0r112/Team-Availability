using MediatR;
using TeamAvailability.Contracts.OnCall;

namespace TeamAvailability.Application.Features.OnCall.GetShifts;

public sealed record GetOnCallShiftsQuery(Guid TeamId) : IRequest<IReadOnlyList<OnCallShiftResponse>>;
