using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetMyPredictions;

/// <summary>Handles <see cref="GetMyPredictionsQuery"/>: fetches and projects the current user's predictions.</summary>
public class GetMyPredictionsQueryHandler
    : IRequestHandler<GetMyPredictionsQuery, Result<IReadOnlyList<PredictionResultDto>>>
{
    private readonly IPredictionRepository _predictions;
    private readonly ICurrentUser _currentUser;

    public GetMyPredictionsQueryHandler(IPredictionRepository predictions, ICurrentUser currentUser)
    {
        _predictions = predictions;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<PredictionResultDto>>> Handle(
        GetMyPredictionsQuery request,
        CancellationToken cancellationToken)
    {
        var userPredictions = await _predictions.GetByUserIdAsync(_currentUser.UserId, cancellationToken);

        var dtos = userPredictions.Select(p => new PredictionResultDto(
            p.Id,
            p.MatchId,
            p.Match.HomeTeam,
            p.Match.AwayTeam,
            p.Match.MatchDate,
            p.PredictedHomeGoals,
            p.PredictedAwayGoals,
            p.Match.HomeGoals,
            p.Match.AwayGoals,
            p.Match.IsFinished,
            p.PointsAwarded
        )).ToList();

        return Result<IReadOnlyList<PredictionResultDto>>.Success(dtos);
    }
}
