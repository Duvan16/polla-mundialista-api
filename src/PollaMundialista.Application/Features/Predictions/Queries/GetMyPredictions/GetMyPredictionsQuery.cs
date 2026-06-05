using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetMyPredictions;

public record GetMyPredictionsQuery : IRequest<Result<IReadOnlyList<PredictionResultDto>>>;
