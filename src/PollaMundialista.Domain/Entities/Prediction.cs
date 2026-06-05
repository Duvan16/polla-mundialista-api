namespace PollaMundialista.Domain.Entities;

public class Prediction
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid MatchId { get; private set; }
    public int PredictedHomeGoals { get; private set; }
    public int PredictedAwayGoals { get; private set; }
    public int? PointsAwarded { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = default!;
    public Match Match { get; private set; } = default!;

    private Prediction() { }

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

    public void UpdatePrediction(int predictedHomeGoals, int predictedAwayGoals)
    {
        PredictedHomeGoals = predictedHomeGoals;
        PredictedAwayGoals = predictedAwayGoals;
    }

    public void AwardPoints(int points)
    {
        PointsAwarded = points;
    }
}
