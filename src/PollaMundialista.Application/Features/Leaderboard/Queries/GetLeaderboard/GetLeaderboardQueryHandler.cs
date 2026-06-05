using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Leaderboard.DTOs;

namespace PollaMundialista.Application.Features.Leaderboard.Queries.GetLeaderboard;

public class GetLeaderboardQueryHandler
    : IRequestHandler<GetLeaderboardQuery, Result<IReadOnlyList<LeaderboardEntryDto>>>
{
    private readonly IPredictionRepository _predictions;

    public GetLeaderboardQueryHandler(IPredictionRepository predictions)
    {
        _predictions = predictions;
    }

    public async Task<Result<IReadOnlyList<LeaderboardEntryDto>>> Handle(
        GetLeaderboardQuery request,
        CancellationToken cancellationToken)
    {
        var predictions = await _predictions.GetAllWithUsersAsync(cancellationToken);

        var ranked = predictions
            .GroupBy(p => new { p.UserId, p.User.DisplayName })
            .Select(g => new
            {
                g.Key.DisplayName,
                TotalPoints = g.Sum(p => p.PointsAwarded ?? 0),
                ExactHits = g.Count(p => p.PointsAwarded == 3)
            })
            .OrderByDescending(x => x.TotalPoints)
            .ThenByDescending(x => x.ExactHits)
            .ThenBy(x => x.DisplayName)
            .ToList();

        var leaderboard = ranked
            .Select((x, i) => new LeaderboardEntryDto(i + 1, x.DisplayName, x.TotalPoints, x.ExactHits))
            .ToList();

        return Result<IReadOnlyList<LeaderboardEntryDto>>.Success(leaderboard);
    }
}
