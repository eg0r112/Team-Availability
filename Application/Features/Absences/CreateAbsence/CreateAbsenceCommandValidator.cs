using FluentValidation;
using TeamAvailability.Domain.Enums;

namespace TeamAvailability.Application.Features.Absences.CreateAbsence;

public sealed class CreateAbsenceCommandValidator : AbstractValidator<CreateAbsenceCommand>
{
    public CreateAbsenceCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.TeamMemberId).NotEmpty();
        RuleFor(x => x.Type).Must(t => Enum.TryParse<AbsenceType>(t, true, out _))
            .WithMessage("Type must be Vacation, Sick, or Other.");
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.ReplacementMemberId)
            .NotEmpty()
            .When(x => string.Equals(x.Type, nameof(AbsenceType.Vacation), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Vacation requires a replacement team member.");
        RuleFor(x => x.ReplacementMemberId)
            .Must((cmd, replacementId) => replacementId != cmd.TeamMemberId)
            .When(x => x.ReplacementMemberId.HasValue)
            .WithMessage("A member cannot be their own replacement.");
    }
}
