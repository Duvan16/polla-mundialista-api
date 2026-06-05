namespace PollaMundialista.Application.Features.Leaderboard.DTOs;

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
