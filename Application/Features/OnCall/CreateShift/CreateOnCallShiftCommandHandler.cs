using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TeamAvailability.Application.Common.Authorization;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Contracts.OnCall;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Exceptions;

namespace TeamAvailability.Application.Features.OnCall.CreateShift;

public sealed class CreateOnCallShiftCommandHandler(
    IApplicationDbContext db,
    ITeamAuthorizationService teamAuthorization,
    IJiraClient jiraClient,
    IJiraIntegrationSettings jiraSettings,
    ILogger<CreateOnCallShiftCommandHandler> logger) : IRequestHandler<CreateOnCallShiftCommand, OnCallShiftResponse>
{
    public async Task<OnCallShiftResponse> Handle(CreateOnCallShiftCommand request, CancellationToken cancellationToken)
    {
        await teamAuthorization.EnsureCanManageTeamAsync(request.TeamId, cancellationToken);

        var member = await db.TeamMembers
            .Include(m => m.User)
            .Include(m => m.Absences.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == request.TeamMemberId && m.TeamId == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team member not found.");

        var team = await db.Teams.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team not found.");

        var startDate = DateOnly.FromDateTime(request.StartUtc.UtcDateTime);
        var endDate = DateOnly.FromDateTime(request.EndUtc.UtcDateTime);
        var onVacation = member.Absences.Any(a => a.Overlaps(startDate, endDate));
        if (onVacation)
            throw new DomainException("Cannot assign on-call shift to a member who is on absence.");

        var shift = OnCallShift.Create(request.TeamId, request.TeamMemberId, request.StartUtc, request.EndUtc);
        db.OnCallShifts.Add(shift);

        string? jiraIssueKey = null;
        string? jiraIssueUrl = null;

        if (jiraSettings.Enabled && jiraClient.IsConfigured
            && !string.IsNullOrWhiteSpace(jiraSettings.ProjectKey)
            && !string.IsNullOrWhiteSpace(jiraSettings.IssueTypeId))
        {
            try
            {
                var summary = $"On-call: {member.User.DisplayName} — {team.Name}";
                var description =
                    $"On-call shift for {member.User.DisplayName} ({member.User.Email}).\n" +
                    $"Team: {team.Name}\n" +
                    $"Period: {request.StartUtc:u} — {request.EndUtc:u} UTC";

                var issue = await jiraClient.CreateOnCallIssueAsync(
                    jiraSettings.ProjectKey,
                    jiraSettings.IssueTypeId,
                    summary,
                    description,
                    member.User.Email,
                    cancellationToken);

                if (issue is not null)
                {
                    shift.LinkJiraIssue(issue.IssueKey);
                    jiraIssueKey = issue.IssueKey;
                    jiraIssueUrl = issue.IssueUrl;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create Jira issue for on-call shift. Shift will be saved without Jira link.");
            }
        }

        return new OnCallShiftResponse(
            shift.Id,
            shift.TeamId,
            shift.TeamMemberId,
            member.User.DisplayName,
            shift.StartUtc,
            shift.EndUtc,
            jiraIssueKey,
            jiraIssueUrl);
    }
}
