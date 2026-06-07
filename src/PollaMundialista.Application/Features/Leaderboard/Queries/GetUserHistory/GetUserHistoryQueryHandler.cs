using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Leaderboard.DTOs;

namespace PollaMundialista.Application.Features.Leaderboard.Queries.GetUserHistory;

/// <summary>
/// Handles <see cref="GetUserHistoryQuery"/>: enforces that users can only view their own history
/// by comparing the requested UserId against the authenticated user's identity.
/// </summary>
public class GetUserHistoryQueryHandler
    : IRequestHandler<GetUserHistoryQuery, Result<IReadOnlyList<UserHistoryItemDto>>>
{
    private readonly IPredictionRepository _predictions;
    private readonly IUserRepository _users;
    private readonly ICurrentUser _currentUser;

    public GetUserHistoryQueryHandler(
        IPredictionRepository predictions,
        IUserRepository users,
        ICurrentUser currentUser)
    {
        _predictions = predictions;
        _users = users;
        _currentUser = currentUser;
    }

    public async Task<Result<IReadOnlyList<UserHistoryItemDto>>> Handle(
        GetUserHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.UserId != _currentUser.UserId)
            return Result<IReadOnlyList<UserHistoryItemDto>>.Failure("Forbidden");

        var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result<IReadOnlyList<UserHistoryItemDto>>.Failure("User not found.");

        var predictions = await _predictions.GetByUserIdAsync(request.UserId, cancellationToken);

        var history = predictions
            .Where(p => p.Match.IsFinished)
            .OrderByDescending(p => p.Match.MatchDate)
            .Select(p => new UserHistoryItemDto(
                p.MatchId,
                p.Match.HomeTeam,
                p.Match.AwayTeam,
                p.Match.MatchDate,
                p.PredictedHomeGoals,
                p.PredictedAwayGoals,
                p.Match.HomeGoals,
                p.Match.AwayGoals,
                p.Match.IsFinished,
                p.PointsAwarded))
            .ToList();

        return Result<IReadOnlyList<UserHistoryItemDto>>.Success(history);
    }
}
