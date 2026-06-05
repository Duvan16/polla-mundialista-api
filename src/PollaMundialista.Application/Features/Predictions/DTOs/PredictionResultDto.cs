namespace PollaMundialista.Application.Features.Predictions.DTOs;

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
