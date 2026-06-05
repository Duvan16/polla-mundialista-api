namespace PollaMundialista.Application.Features.Leaderboard.DTOs;

public record LeaderboardEntryDto(
    int Rank,
    string DisplayName,
    int TotalPoints,
    int ExactHits);
