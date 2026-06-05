using FluentValidation;

namespace PollaMundialista.Application.Features.Admin.Commands.SetMatchResult;

public class SetMatchResultCommandValidator : AbstractValidator<SetMatchResultCommand>
{
    public SetMatchResultCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("MatchId is required.");

        RuleFor(x => x.HomeGoals)
            .GreaterThanOrEqualTo(0).WithMessage("Home goals cannot be negative.");

        RuleFor(x => x.AwayGoals)
            .GreaterThanOrEqualTo(0).WithMessage("Away goals cannot be negative.");
    }
}
