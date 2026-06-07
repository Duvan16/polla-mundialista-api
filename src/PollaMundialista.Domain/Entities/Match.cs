namespace PollaMundialista.Domain.Entities;

/// <summary>
/// Represents a World Cup fixture between two teams, including its result once played.
/// </summary>
public class Match
{
    public Guid Id { get; private set; }
    public string GroupName { get; private set; } = default!;
    public string HomeTeam { get; private set; } = default!;
    public string AwayTeam { get; private set; } = default!;
    public DateTime MatchDate { get; private set; }

    /// <summary>Null until <see cref="SetResult"/> is called by an Admin.</summary>
    public int? HomeGoals { get; private set; }

    /// <summary>Null until <see cref="SetResult"/> is called by an Admin.</summary>
    public int? AwayGoals { get; private set; }

    /// <summary>
    /// True once the Admin has recorded a result. Predictions are rejected once this is set.
    /// </summary>
    public bool IsFinished { get; private set; }

    public ICollection<Prediction> Predictions { get; private set; } = new List<Prediction>();

    private Match() { }

    /// <summary>Creates a new unplayed match.</summary>
    public static Match Create(string groupName, string homeTeam, string awayTeam, DateTime matchDate)
    {
        return new Match
        {
            Id = Guid.NewGuid(),
            GroupName = groupName,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            MatchDate = matchDate,
            IsFinished = false
        };
    }

    /// <summary>
    /// Records the final score and marks the match as finished, enabling point calculation.
    /// </summary>
    /// <remarks>Calling this multiple times is safe (idempotent overwrite).</remarks>
    public void SetResult(int homeGoals, int awayGoals)
    {
        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        IsFinished = true;
    }
}
