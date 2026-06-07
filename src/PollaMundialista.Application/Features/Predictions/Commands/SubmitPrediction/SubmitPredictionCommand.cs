using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Commands.SubmitPrediction;

/// <summary>Creates or updates the authenticated user's score prediction for a given match.</summary>
public record SubmitPredictionCommand(
    Guid MatchId,
    int PredictedHomeGoals,
    int PredictedAwayGoals
) : IRequest<Result<SubmitPredictionResponse>>;
