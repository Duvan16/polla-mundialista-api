namespace PollaMundialista.Application.Features.Leaderboard.DTOs;

public record LeaderboardEntryDto(
    Guid UserId,
    int Rank,
    string DisplayName,
    int TotalPoints,
    int ExactHits);
