namespace PollaMundialista.Domain.Entities;

/// <summary>
/// A user's score prediction for a single match. One prediction per user per match is enforced by a unique DB constraint.
/// </summary>
/// <remarks>
/// The uniqueness constraint (UserId + MatchId) exists so users cannot circumvent the
/// "update, not create" rule by submitting duplicate rows through concurrent requests.
/// </remarks>
public class Prediction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MatchId { get; private set; }
    public int PredictedHomeGoals { get; private set; }
    public int PredictedAwayGoals { get; private set; }

    /// <summary>
    /// Points earned for this prediction. Null until the match result is set and
    /// <see cref="AwardPoints"/> is called during score recalculation.
    /// </summary>
    public int? PointsAwarded { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = default!;
    public Match Match { get; private set; } = default!;

    private Prediction() { }

    /// <summary>Creates a new prediction. The match must not be finished at call time.</summary>
    public static Prediction Create(Guid userId, Guid matchId, int predictedHomeGoals, int predictedAwayGoals)
    {
        return new Prediction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MatchId = matchId,
            PredictedHomeGoals = predictedHomeGoals,
            PredictedAwayGoals = predictedAwayGoals,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>Replaces the predicted score. Only valid while the match is not finished.</summary>
    public void UpdatePrediction(int predictedHomeGoals, int predictedAwayGoals)
    {
        PredictedHomeGoals = predictedHomeGoals;
        PredictedAwayGoals = predictedAwayGoals;
    }

    /// <summary>Sets the points earned after a match result is recorded. Called during bulk recalculation.</summary>
    public void AwardPoints(int points)
    {
        PointsAwarded = points;
    }
}
