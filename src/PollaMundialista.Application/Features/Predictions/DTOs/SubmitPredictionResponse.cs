namespace PollaMundialista.Application.Features.Predictions.DTOs;

/// <summary>Confirmation returned after a prediction is successfully created or updated.</summary>
public record SubmitPredictionResponse(
    Guid PredictionId,
    Guid MatchId,
    string HomeTeam,
    string AwayTeam,
    int PredictedHomeGoals,
    int PredictedAwayGoals
);
