using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetUpcomingMatches;

public record GetUpcomingMatchesQuery : IRequest<Result<IReadOnlyList<MatchWithPredictionDto>>>;
