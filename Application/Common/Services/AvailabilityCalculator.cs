using TeamAvailability.Contracts.Availability;
using TeamAvailability.Domain.Entities;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Application.Common.Services;

public static class AvailabilityCalculator
{
    public static TeamAvailabilityResponse Calculate(Guid teamId, DateOnly date, IReadOnlyList<TeamMember> members)
    {
        var memberDtos = members.Select(member =>
        {
            var absence = member.Absences
                .Where(a => !a.IsDeleted && a.Overlaps(date, date))
                .OrderByDescending(a => a.Type == AbsenceType.Vacation)
                .FirstOrDefault();

            var isAvailable = absence is null;
            var presence = member.Presence?.Status ?? PresenceStatus.Away;

            return new MemberAvailabilityDto(
                member.Id,
                member.UserId,
                member.User.DisplayName,
                member.TimeZoneId,
                isAvailable,
                absence?.Type.ToString(),
                absence?.ReplacementMember?.User.DisplayName,
                presence.ToString());
        }).ToList();

        return new TeamAvailabilityResponse(teamId, date, memberDtos);
    }
}
