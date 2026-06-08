using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetUpcomingMatches;

/// <summary>
/// Handles <see cref="GetUpcomingMatchesQuery"/>: merges match data with the user's predictions
/// via an in-memory dictionary to avoid an N+1 query.
/// </summary>
public class GetUpcomingMatchesQueryHandler
    : IRequestHandler<GetUpcomingMatchesQuery, Result<IReadOnlyList<MatchWithPredictionDto>>>
{
    private readonly IMatchRepository _matches;
    private readonly IPredictionRepository _predictions;
    private readonly ICurrentUser _currentUser;

    public GetUpcomingMatchesQueryHandler(
        IMatchRepository matches,
        IPredictionRepository predictions,
        ICurrentUser currentUser)
    {
        _matches = matches;
        _predictions = predictions;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<MatchWithPredictionDto>>> Handle(
        GetUpcomingMatchesQuery request,
        CancellationToken cancellationToken)
    {
        var allMatches = await _matches.GetAllAsync(cancellationToken);

        Dictionary<Guid, (int home, int away, int? points)> myPredictions = [];

        if (_currentUser.IsAuthenticated)
        {
            var userPredictions = await _predictions.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
            myPredictions = userPredictions.ToDictionary(
                p => p.MatchId,
                p => (p.PredictedHomeGoals, p.PredictedAwayGoals, p.PointsAwarded));
        }

        var dtos = allMatches.Where(m => !m.IsFinished).OrderBy(m => m.MatchDate).Select(m =>
        {
            myPredictions.TryGetValue(m.Id, out var pred);
            var hasPrediction = myPredictions.ContainsKey(m.Id);
            return new MatchWithPredictionDto(
                m.Id,
                m.GroupName,
                m.HomeTeam,
                m.AwayTeam,
                m.MatchDate,
                hasPrediction ? pred.home : null,
                hasPrediction ? pred.away : null,
                m.IsFinished,
                m.HomeGoals,
                m.AwayGoals,
                hasPrediction ? pred.points : null);
        }).ToList();

        return Result<IReadOnlyList<MatchWithPredictionDto>>.Success(dtos);
    }
}
