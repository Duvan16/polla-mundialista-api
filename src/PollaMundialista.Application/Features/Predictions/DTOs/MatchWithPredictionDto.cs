namespace PollaMundialista.Application.Features.Predictions.DTOs;

/// <summary>
/// A match enriched with the requesting user's prediction (if any) and the actual result (once finished).
/// Null prediction fields indicate no prediction was submitted by this user.
/// </summary>
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
