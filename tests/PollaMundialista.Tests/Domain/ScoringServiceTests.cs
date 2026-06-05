using FluentAssertions;
using PollaMundialista.Domain.Services;

namespace PollaMundialista.Tests.Domain;

public class ScoringServiceTests
{
    // ── Exact score (3 points) ────────────────────────────────────────────────

    [Fact]
    public void CalculatePoints_ExactScore_HomeWin_Returns3()
    {
        ScoringService.CalculatePoints(2, 0, 2, 0).Should().Be(3);
    }

    [Fact]
    public void CalculatePoints_ExactScore_AwayWin_Returns3()
    {
        ScoringService.CalculatePoints(0, 3, 0, 3).Should().Be(3);
    }

    [Fact]
    public void CalculatePoints_ExactScore_Draw_Returns3()
    {
        ScoringService.CalculatePoints(1, 1, 1, 1).Should().Be(3);
    }

    [Fact]
    public void CalculatePoints_ExactScore_ZeroZeroDraw_Returns3()
    {
        ScoringService.CalculatePoints(0, 0, 0, 0).Should().Be(3);
    }

    [Fact]
    public void CalculatePoints_ExactScore_HighScoring_Returns3()
    {
        ScoringService.CalculatePoints(4, 3, 4, 3).Should().Be(3);
    }

    // ── Correct outcome only (1 point) ────────────────────────────────────────

    [Fact]
    public void CalculatePoints_CorrectOutcome_HomeWin_WrongScore_Returns1()
    {
        ScoringService.CalculatePoints(1, 0, 3, 1).Should().Be(1);
    }

    [Fact]
    public void CalculatePoints_CorrectOutcome_AwayWin_WrongScore_Returns1()
    {
        ScoringService.CalculatePoints(0, 2, 1, 4).Should().Be(1);
    }

    [Fact]
    public void CalculatePoints_CorrectOutcome_Draw_WrongScore_Returns1()
    {
        ScoringService.CalculatePoints(2, 2, 0, 0).Should().Be(1);
    }

    [Fact]
    public void CalculatePoints_CorrectOutcome_Draw_DifferentGoals_Returns1()
    {
        ScoringService.CalculatePoints(1, 1, 3, 3).Should().Be(1);
    }

    [Fact]
    public void CalculatePoints_CorrectOutcome_HomeWin_LargeMarginDifference_Returns1()
    {
        ScoringService.CalculatePoints(1, 0, 5, 0).Should().Be(1);
    }

    [Fact]
    public void CalculatePoints_CorrectOutcome_AwayWin_PredictedByOneActualByMany_Returns1()
    {
        ScoringService.CalculatePoints(0, 1, 0, 5).Should().Be(1);
    }

    // ── Wrong outcome (0 points) ───────────────────────────────────────────────

    [Fact]
    public void CalculatePoints_WrongOutcome_PredictedHomeWin_ActualAwayWin_Returns0()
    {
        ScoringService.CalculatePoints(2, 1, 0, 1).Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_WrongOutcome_PredictedAwayWin_ActualHomeWin_Returns0()
    {
        ScoringService.CalculatePoints(0, 2, 2, 0).Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_WrongOutcome_PredictedDraw_ActualHomeWin_Returns0()
    {
        ScoringService.CalculatePoints(1, 1, 2, 0).Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_WrongOutcome_PredictedDraw_ActualAwayWin_Returns0()
    {
        ScoringService.CalculatePoints(0, 0, 0, 1).Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_WrongOutcome_PredictedHomeWin_ActualDraw_Returns0()
    {
        ScoringService.CalculatePoints(2, 0, 1, 1).Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_WrongOutcome_PredictedAwayWin_ActualDraw_Returns0()
    {
        ScoringService.CalculatePoints(0, 3, 2, 2).Should().Be(0);
    }

    // ── Draw edge cases ────────────────────────────────────────────────────────

    [Fact]
    public void CalculatePoints_BothZero_PredictedHomeWin_Returns0()
    {
        // Predicted 1-0 home win, actual 0-0 draw
        ScoringService.CalculatePoints(1, 0, 0, 0).Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_PredictedDraw_ExactZeroZero_Returns3()
    {
        ScoringService.CalculatePoints(0, 0, 0, 0).Should().Be(3);
    }

    // ── Theory: all scoring branches ──────────────────────────────────────────

    [Theory]
    [InlineData(2, 1, 2, 1, 3)]  // exact home win
    [InlineData(0, 2, 0, 2, 3)]  // exact away win
    [InlineData(1, 1, 1, 1, 3)]  // exact draw
    [InlineData(1, 0, 2, 0, 1)]  // correct home win outcome
    [InlineData(0, 1, 0, 3, 1)]  // correct away win outcome
    [InlineData(2, 2, 0, 0, 1)]  // correct draw outcome
    [InlineData(2, 0, 0, 1, 0)]  // home predicted, away won
    [InlineData(0, 2, 1, 0, 0)]  // away predicted, home won
    [InlineData(1, 1, 2, 0, 0)]  // draw predicted, home won
    [InlineData(2, 0, 0, 0, 0)]  // home predicted, draw actual
    public void CalculatePoints_Theory_AllBranches(
        int predHome, int predAway,
        int actHome, int actAway,
        int expectedPoints)
    {
        ScoringService.CalculatePoints(predHome, predAway, actHome, actAway)
            .Should().Be(expectedPoints);
    }

    // ── Return values are constrained to {0, 1, 3} ────────────────────────────

    [Theory]
    [InlineData(0, 0, 1, 0)]
    [InlineData(3, 0, 0, 3)]
    [InlineData(1, 2, 2, 1)]
    [InlineData(0, 0, 0, 1)]
    [InlineData(5, 5, 5, 5)]
    public void CalculatePoints_AlwaysReturnsValidScore(
        int predHome, int predAway, int actHome, int actAway)
    {
        var points = ScoringService.CalculatePoints(predHome, predAway, actHome, actAway);
        points.Should().BeOneOf(0, 1, 3);
    }
}
