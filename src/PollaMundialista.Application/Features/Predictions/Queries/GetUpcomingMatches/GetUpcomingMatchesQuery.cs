using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetUpcomingMatches;

/// <summary>
/// Returns all matches ordered by date, with the authenticated user's existing prediction embedded per match (if any).
/// </summary>
public record GetUpcomingMatchesQuery : IRequest<Result<IReadOnlyList<MatchWithPredictionDto>>>;
