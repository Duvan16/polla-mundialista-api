namespace PollaMundialista.Application.Features.Leaderboard.DTOs;

/// <summary>A single participant's position on the leaderboard.</summary>
public record LeaderboardEntryDto(
    Guid UserId,
    int Rank,
    string DisplayName,
    int TotalPoints,

    /// <summary>Number of predictions where the exact scoreline was guessed correctly (3-point hits).</summary>
    int ExactHits);
