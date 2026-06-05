using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Predictions.Commands.SubmitPrediction;

public record SubmitPredictionCommand(
    Guid MatchId,
    int PredictedHomeGoals,
    int PredictedAwayGoals
) : IRequest<Result<SubmitPredictionResponse>>;
