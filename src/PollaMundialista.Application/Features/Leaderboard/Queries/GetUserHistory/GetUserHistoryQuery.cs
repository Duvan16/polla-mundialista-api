using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Leaderboard.DTOs;

namespace PollaMundialista.Application.Features.Leaderboard.Queries.GetUserHistory;

public record GetUserHistoryQuery(Guid UserId) : IRequest<Result<IReadOnlyList<UserHistoryItemDto>>>;
