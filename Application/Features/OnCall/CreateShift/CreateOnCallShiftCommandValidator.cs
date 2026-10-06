using FluentValidation;

namespace TeamAvailability.Application.Features.OnCall.CreateShift;

public sealed class CreateOnCallShiftCommandValidator : AbstractValidator<CreateOnCallShiftCommand>
{
    public CreateOnCallShiftCommandValidator()
    {
        RuleFor(x => x.TeamId).NotEmpty();
        RuleFor(x => x.TeamMemberId).NotEmpty();
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
    }
}
