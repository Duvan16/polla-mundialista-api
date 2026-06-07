using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Domain.Services;

namespace PollaMundialista.Application.Features.Admin.Commands.SetMatchResult;

/// <summary>
/// Handles <see cref="SetMatchResultCommand"/>: sets the match result and recalculates
/// <c>PointsAwarded</c> for every prediction on that match. Operation is idempotent.
/// </summary>
public class SetMatchResultCommandHandler : IRequestHandler<SetMatchResultCommand, Result>
{
    private readonly IMatchRepository _matches;
    private readonly IPredictionRepository _predictions;
    private readonly IUnitOfWork _uow;

    public SetMatchResultCommandHandler(
        IMatchRepository matches,
        IPredictionRepository predictions,
        IUnitOfWork uow)
    {
        _matches = matches;
        _predictions = predictions;
        _uow = uow;
    }

    public async Task<Result> Handle(SetMatchResultCommand request, CancellationToken cancellationToken)
    {
        var match = await _matches.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            return Result.Failure("Match not found.");

        match.SetResult(request.HomeGoals, request.AwayGoals);

        var predictions = await _predictions.GetByMatchIdAsync(request.MatchId, cancellationToken);
        foreach (var prediction in predictions)
        {
            var points = ScoringService.CalculatePoints(
                prediction.PredictedHomeGoals,
                prediction.PredictedAwayGoals,
                request.HomeGoals,
                request.AwayGoals);
            prediction.AwardPoints(points);
        }

        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
