using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TeamAvailability.Application.Common.Authorization;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Application.Events;
using TeamAvailability.Contracts.Absences;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Enums;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.Absences.CreateAbsence;

public sealed class CreateAbsenceCommandHandler(
    IApplicationDbContext db,
    ITeamAuthorizationService teamAuthorization,
    ICurrentUserService currentUser,
    ICorrelationIdProvider correlationIdProvider) : IRequestHandler<CreateAbsenceCommand, AbsenceResponse>
{
    public async Task<AbsenceResponse> Handle(CreateAbsenceCommand request, CancellationToken cancellationToken)
    {
        await teamAuthorization.EnsureCanManageTeamAsync(request.TeamId, cancellationToken);

        var member = await db.TeamMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == request.TeamMemberId && m.TeamId == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team member not found.");

        var type = Enum.Parse<AbsenceType>(request.Type, true);

        if (type == AbsenceType.Vacation && request.ReplacementMemberId is null)
            throw new DomainException("Vacation requires a replacement team member.");

        if (request.ReplacementMemberId.HasValue)
        {
            var replacement = await db.TeamMembers
                .Include(m => m.User)
                .Include(m => m.Absences.Where(a => !a.IsDeleted))
                .FirstOrDefaultAsync(m => m.Id == request.ReplacementMemberId && m.TeamId == request.TeamId, cancellationToken)
                ?? throw new NotFoundException("Replacement member not found in this team.");

            var replacementOnVacation = replacement.Absences
                .Any(a => a.Type == AbsenceType.Vacation && a.Overlaps(request.StartDate, request.EndDate));
            if (replacementOnVacation)
                throw new DomainException("Replacement member is unavailable during the requested period.");
        }

        var overlapping = await db.Absences
            .AnyAsync(a => !a.IsDeleted
                && a.TeamMemberId == request.TeamMemberId
                && a.StartDate <= request.EndDate
                && request.StartDate <= a.EndDate, cancellationToken);
        if (overlapping)
            throw new ConflictException("Member already has an absence overlapping these dates.");

        var absence = Absence.Create(
            request.TeamMemberId,
            type,
            request.StartDate,
            request.EndDate,
            request.ReplacementMemberId,
            request.Notes);

        db.Absences.Add(absence);

        db.AuditEntries.Add(AuditEntry.Create(
            nameof(Absence),
            absence.Id,
            "Created",
            currentUser.UserId,
            JsonSerializer.Serialize(new { request.Type, request.StartDate, request.EndDate }),
            correlationIdProvider.CorrelationId));

        var integrationEvent = new AbsenceCreatedIntegrationEvent(
            absence.Id,
            request.TeamId,
            request.TeamMemberId,
            type.ToString(),
            request.StartDate,
            request.EndDate,
            request.ReplacementMemberId);

        db.OutboxMessages.Add(OutboxMessage.Create(
            nameof(AbsenceCreatedIntegrationEvent),
            JsonSerializer.Serialize(integrationEvent)));

        TeamMember? replacementMember = null;
        if (request.ReplacementMemberId.HasValue)
        {
            replacementMember = await db.TeamMembers
                .Include(m => m.User)
                .FirstAsync(m => m.Id == request.ReplacementMemberId, cancellationToken);
        }

        return new AbsenceResponse(
            absence.Id,
            member.Id,
            member.User.DisplayName,
            type.ToString(),
            absence.StartDate,
            absence.EndDate,
            request.ReplacementMemberId,
            replacementMember?.User.DisplayName,
            absence.Notes);
    }
}
