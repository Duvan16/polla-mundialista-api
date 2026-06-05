using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetUpcomingMatches;

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
        var upcoming = allMatches.Where(m => !m.IsFinished).ToList();

        Dictionary<Guid, (int home, int away)> myPredictions = [];

        if (_currentUser.IsAuthenticated)
        {
            var userPredictions = await _predictions.GetByUserIdAsync(_currentUser.UserId, cancellationToken);
            myPredictions = userPredictions.ToDictionary(
                p => p.MatchId,
                p => (p.PredictedHomeGoals, p.PredictedAwayGoals));
        }

        var dtos = upcoming.Select(m =>
        {
            myPredictions.TryGetValue(m.Id, out var pred);
            return new MatchWithPredictionDto(
                m.Id,
                m.GroupName,
                m.HomeTeam,
                m.AwayTeam,
                m.MatchDate,
                myPredictions.ContainsKey(m.Id) ? pred.home : null,
                myPredictions.ContainsKey(m.Id) ? pred.away : null);
        }).ToList();

        return Result<IReadOnlyList<MatchWithPredictionDto>>.Success(dtos);
    }
}
