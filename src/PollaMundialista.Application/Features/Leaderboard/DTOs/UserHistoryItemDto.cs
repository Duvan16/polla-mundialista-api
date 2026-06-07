namespace PollaMundialista.Application.Features.Leaderboard.DTOs;

/// <summary>A single finished match in a user's prediction history, showing what they predicted vs. the actual result.</summary>
public record UserHistoryItemDto(
    Guid MatchId,
    string HomeTeam,
    string AwayTeam,
    DateTime MatchDate,
    int PredictedHomeGoals,
    int PredictedAwayGoals,
    int? ActualHomeGoals,
    int? ActualAwayGoals,
    bool IsFinished,
    int? PointsAwarded);
