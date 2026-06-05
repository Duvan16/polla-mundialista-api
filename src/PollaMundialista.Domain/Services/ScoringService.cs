namespace PollaMundialista.Domain.Services;

public static class ScoringService
{
    public static int CalculatePoints(
        int predictedHome, int predictedAway,
        int actualHome, int actualAway)
    {
        if (predictedHome == actualHome && predictedAway == actualAway)
            return 3;

        var predictedOutcome = Math.Sign(predictedHome - predictedAway);
        var actualOutcome = Math.Sign(actualHome - actualAway);

        return predictedOutcome == actualOutcome ? 1 : 0;
    }
}
