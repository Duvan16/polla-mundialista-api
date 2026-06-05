using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Admin.Commands.SetMatchResult;
using PollaMundialista.Domain.Entities;
using Match = PollaMundialista.Domain.Entities.Match;

namespace PollaMundialista.Tests.Application.Admin;

public class SetMatchResultHandlerTests
{
    private readonly Mock<IMatchRepository> _matches = new();
    private readonly Mock<IPredictionRepository> _predictions = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private SetMatchResultCommandHandler CreateHandler() =>
        new(_matches.Object, _predictions.Object, _uow.Object);

    private static Match MakeMatch(bool isFinished = false)
    {
        var match = Match.Create("Group A", "Argentina", "Brazil",
            new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc));
        if (isFinished) match.SetResult(1, 0);
        return match;
    }

    private static Prediction MakePrediction(Guid matchId, int home, int away)
        => Prediction.Create(Guid.NewGuid(), matchId, home, away);

    [Fact]
    public async Task Handle_MatchNotFound_ReturnsFailure()
    {
        _matches.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Match?)null);

        var result = await CreateHandler().Handle(
            new SetMatchResultCommand(Guid.NewGuid(), 1, 0), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Match not found.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NoExistingPredictions_SetsResultAndSaves()
    {
        var match = MakeMatch();
        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>());

        var result = await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 2, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        match.IsFinished.Should().BeTrue();
        match.HomeGoals.Should().Be(2);
        match.AwayGoals.Should().Be(1);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleUsers_ExactScoreGets3Points()
    {
        // Actual result: 2-1
        var match = MakeMatch();
        var exactPrediction = MakePrediction(match.Id, 2, 1);
        var wrongPrediction = MakePrediction(match.Id, 0, 3);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { exactPrediction, wrongPrediction });

        var result = await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 2, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        exactPrediction.PointsAwarded.Should().Be(3);
        wrongPrediction.PointsAwarded.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultipleUsers_CorrectOutcomeGets1Point()
    {
        // Actual result: 2-1 (home win)
        var match = MakeMatch();
        var exactPrediction = MakePrediction(match.Id, 2, 1);   // exact → 3
        var outcomePrediction = MakePrediction(match.Id, 1, 0); // home win → 1
        var wrongPrediction = MakePrediction(match.Id, 0, 1);   // away win → 0

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { exactPrediction, outcomePrediction, wrongPrediction });

        var result = await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 2, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        exactPrediction.PointsAwarded.Should().Be(3);
        outcomePrediction.PointsAwarded.Should().Be(1);
        wrongPrediction.PointsAwarded.Should().Be(0);
    }

    [Fact]
    public async Task Handle_DrawResult_DrawPredictionGets3Points()
    {
        // Actual result: 1-1 (draw)
        var match = MakeMatch();
        var exactDraw = MakePrediction(match.Id, 1, 1);         // exact draw → 3
        var otherDraw = MakePrediction(match.Id, 0, 0);         // correct outcome (draw) → 1
        var homeWinPrediction = MakePrediction(match.Id, 2, 0); // wrong outcome → 0

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { exactDraw, otherDraw, homeWinPrediction });

        var result = await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 1, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        exactDraw.PointsAwarded.Should().Be(3);
        otherDraw.PointsAwarded.Should().Be(1);
        homeWinPrediction.PointsAwarded.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Idempotent_ResettingResultRecalculatesPoints()
    {
        // First result: 2-1. Then re-set to 0-0 (draw). Points must recalculate correctly.
        var match = MakeMatch();
        var prediction = MakePrediction(match.Id, 2, 1); // exact for 2-1; wrong for 0-0

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { prediction });

        // First set: 2-1 → prediction gets 3 points
        await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 2, 1), CancellationToken.None);
        prediction.PointsAwarded.Should().Be(3);

        // Re-set: 0-0 (draw) → same prediction now gets 0 points
        await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 0, 0), CancellationToken.None);

        prediction.PointsAwarded.Should().Be(0);
        match.HomeGoals.Should().Be(0);
        match.AwayGoals.Should().Be(0);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_Idempotent_SameResultTwiceYieldsSamePoints()
    {
        var match = MakeMatch();
        var prediction = MakePrediction(match.Id, 1, 0); // home win

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { prediction });

        await CreateHandler().Handle(new SetMatchResultCommand(match.Id, 2, 1), CancellationToken.None);
        await CreateHandler().Handle(new SetMatchResultCommand(match.Id, 2, 1), CancellationToken.None);

        prediction.PointsAwarded.Should().Be(1); // home win → 1 point, consistently
    }

    [Fact]
    public async Task Handle_MultipleUsers_AllGetCorrectPointsForDraw()
    {
        // Actual: 0-0
        var match = MakeMatch();
        var user1 = MakePrediction(match.Id, 0, 0); // exact → 3
        var user2 = MakePrediction(match.Id, 1, 1); // draw (outcome correct) → 1
        var user3 = MakePrediction(match.Id, 2, 1); // home win → 0
        var user4 = MakePrediction(match.Id, 0, 2); // away win → 0

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByMatchIdAsync(match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { user1, user2, user3, user4 });

        var result = await CreateHandler().Handle(
            new SetMatchResultCommand(match.Id, 0, 0), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user1.PointsAwarded.Should().Be(3);
        user2.PointsAwarded.Should().Be(1);
        user3.PointsAwarded.Should().Be(0);
        user4.PointsAwarded.Should().Be(0);
    }
}
