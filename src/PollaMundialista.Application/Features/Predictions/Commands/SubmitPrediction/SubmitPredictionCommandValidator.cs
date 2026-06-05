using FluentValidation;

namespace PollaMundialista.Application.Features.Predictions.Commands.SubmitPrediction;

public class SubmitPredictionCommandValidator : AbstractValidator<SubmitPredictionCommand>
{
    public SubmitPredictionCommandValidator()
    {
        RuleFor(x => x.MatchId)
            .NotEmpty().WithMessage("MatchId is required.");

        RuleFor(x => x.PredictedHomeGoals)
            .GreaterThanOrEqualTo(0).WithMessage("Predicted home goals cannot be negative.");

        RuleFor(x => x.PredictedAwayGoals)
            .GreaterThanOrEqualTo(0).WithMessage("Predicted away goals cannot be negative.");
    }
}
