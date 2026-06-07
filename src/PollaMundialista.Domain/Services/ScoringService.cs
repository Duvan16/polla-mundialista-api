namespace PollaMundialista.Domain.Services;

/// <summary>
/// Calculates prediction points according to the 3/1/0 scoring rules.
/// </summary>
/// <remarks>
/// Rules: 3 points for an exact scoreline, 1 point for predicting the correct
/// outcome (home win / draw / away win) with a wrong score, 0 points otherwise.
/// Uses <see cref="Math.Sign"/> to derive outcome so all draw scores (0-0, 1-1, …)
/// and all home/away wins are treated uniformly.
/// </remarks>
public static class ScoringService
{
    /// <summary>
    /// Returns the points earned for a single prediction against the actual result.
    /// </summary>
    /// <param name="predictedHome">Goals the user predicted for the home team.</param>
    /// <param name="predictedAway">Goals the user predicted for the away team.</param>
    /// <param name="actualHome">Actual goals scored by the home team.</param>
    /// <param name="actualAway">Actual goals scored by the away team.</param>
    /// <returns>3 for exact score, 1 for correct outcome only, 0 for wrong outcome.</returns>
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
