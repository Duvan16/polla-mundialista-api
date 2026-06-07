using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Leaderboard.DTOs;

namespace PollaMundialista.Application.Features.Leaderboard.Queries.GetLeaderboard;

/// <summary>Returns all participants ranked by total points, with exact-score count as a tiebreaker.</summary>
public record GetLeaderboardQuery : IRequest<Result<IReadOnlyList<LeaderboardEntryDto>>>;
