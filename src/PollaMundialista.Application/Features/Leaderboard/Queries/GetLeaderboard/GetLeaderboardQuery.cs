using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Leaderboard.DTOs;

namespace PollaMundialista.Application.Features.Leaderboard.Queries.GetLeaderboard;

public record GetLeaderboardQuery : IRequest<Result<IReadOnlyList<LeaderboardEntryDto>>>;
