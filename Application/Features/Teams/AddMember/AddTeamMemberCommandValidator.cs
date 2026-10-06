using FluentValidation;

namespace TeamAvailability.Application.Features.Teams.AddMember;

public sealed class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TimeZoneId).NotEmpty().Must(BeValidTimeZone).WithMessage("Invalid IANA time zone.");
        RuleFor(x => x.Role).Must(r => r is "Member" or "TeamLead").WithMessage("Role must be Member or TeamLead.");
    }

    private static bool BeValidTimeZone(string timeZoneId)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
