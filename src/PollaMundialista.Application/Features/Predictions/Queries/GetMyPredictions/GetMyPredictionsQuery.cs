using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Queries.GetMyPredictions;

/// <summary>Returns all predictions made by the currently authenticated user, including points for finished matches.</summary>
public record GetMyPredictionsQuery : IRequest<Result<IReadOnlyList<PredictionResultDto>>>;
