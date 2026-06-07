using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Leaderboard.DTOs;

namespace PollaMundialista.Application.Features.Leaderboard.Queries.GetUserHistory;

/// <summary>Returns the prediction history for a specific user, limited to finished matches only.</summary>
public record GetUserHistoryQuery(Guid UserId) : IRequest<Result<IReadOnlyList<UserHistoryItemDto>>>;
