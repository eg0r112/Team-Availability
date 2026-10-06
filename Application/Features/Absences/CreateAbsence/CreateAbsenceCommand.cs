using MediatR;
using TeamAvailability.Application.Common.Behaviors;
using TeamAvailability.Contracts.Absences;

namespace TeamAvailability.Application.Features.Absences.CreateAbsence;

public sealed record CreateAbsenceCommand(
    Guid TeamId,
    Guid TeamMemberId,
    string Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ReplacementMemberId,
    string? Notes) : IRequest<AbsenceResponse>, ITransactionalRequest;
