namespace PollaMundialista.Application.Features.Predictions.DTOs;

public record SubmitPredictionResponse(
    Guid PredictionId,
    Guid MatchId,
    string HomeTeam,
    string AwayTeam,
    int PredictedHomeGoals,
    int PredictedAwayGoals
);
