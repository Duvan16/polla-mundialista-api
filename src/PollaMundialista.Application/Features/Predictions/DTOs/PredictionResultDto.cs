namespace PollaMundialista.Application.Features.Predictions.DTOs;

/// <summary>A user's prediction for a specific match, paired with the actual result and points earned.</summary>
public record PredictionResultDto(
    Guid PredictionId,
    Guid MatchId,
    string HomeTeam,
    string AwayTeam,
    DateTime MatchDate,
    int PredictedHomeGoals,
    int PredictedAwayGoals,
    int? ActualHomeGoals,
    int? ActualAwayGoals,
    bool IsFinished,
    int? PointsAwarded
);
