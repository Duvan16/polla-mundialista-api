namespace PollaMundialista.Application.Features.Predictions.DTOs;

public record MatchWithPredictionDto(
    Guid MatchId,
    string GroupName,
    string HomeTeam,
    string AwayTeam,
    DateTime MatchDate,
    int? MyPredictedHomeGoals,
    int? MyPredictedAwayGoals,
    bool IsFinished = false,
    int? ActualHomeGoals = null,
    int? ActualAwayGoals = null,
    int? PointsAwarded = null
);
