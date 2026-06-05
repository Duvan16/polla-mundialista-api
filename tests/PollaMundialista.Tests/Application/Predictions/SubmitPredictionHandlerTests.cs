using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.Commands.SubmitPrediction;
using PollaMundialista.Domain.Entities;
using Match = PollaMundialista.Domain.Entities.Match;

namespace PollaMundialista.Tests.Application.Predictions;

public class SubmitPredictionHandlerTests
{
    private readonly Mock<IMatchRepository> _matches = new();
    private readonly Mock<IPredictionRepository> _predictions = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly Guid _userId = Guid.NewGuid();

    public SubmitPredictionHandlerTests()
    {
        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(_userId);
    }

    private SubmitPredictionCommandHandler CreateHandler() =>
        new(_matches.Object, _predictions.Object, _uow.Object, _currentUser.Object);

    private static Match MakeMatch(bool isFinished = false)
    {
        var match = Match.Create("Group A", "Argentina", "Brazil",
            new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc));
        if (isFinished) match.SetResult(1, 0);
        return match;
    }

    [Fact]
    public async Task Handle_NewPrediction_CreatesAndSaves()
    {
        var match = MakeMatch();
        var command = new SubmitPredictionCommand(match.Id, 2, 1);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByUserAndMatchAsync(_userId, match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Prediction?)null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PredictedHomeGoals.Should().Be(2);
        result.Value.PredictedAwayGoals.Should().Be(1);
        result.Value.HomeTeam.Should().Be("Argentina");

        _predictions.Verify(r => r.AddAsync(It.IsAny<Prediction>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingPrediction_UpdatesWithoutDuplicate()
    {
        var match = MakeMatch();
        var existing = Prediction.Create(_userId, match.Id, 1, 0);
        var command = new SubmitPredictionCommand(match.Id, 3, 2);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByUserAndMatchAsync(_userId, match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(existing);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PredictedHomeGoals.Should().Be(3);
        result.Value.PredictedAwayGoals.Should().Be(2);

        // No new entity added — updated in-place via EF change tracking
        _predictions.Verify(r => r.AddAsync(It.IsAny<Prediction>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MatchNotFound_ReturnsFailure()
    {
        var command = new SubmitPredictionCommand(Guid.NewGuid(), 1, 0);

        _matches.Setup(r => r.GetByIdAsync(command.MatchId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Match?)null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Match not found.");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FinishedMatch_ReturnsFailure()
    {
        var match = MakeMatch(isFinished: true);
        var command = new SubmitPredictionCommand(match.Id, 1, 0);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("finished");
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FinishedMatch_NeverCallsRepository()
    {
        var match = MakeMatch(isFinished: true);
        var command = new SubmitPredictionCommand(match.Id, 2, 2);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);

        await CreateHandler().Handle(command, CancellationToken.None);

        _predictions.Verify(r => r.GetByUserAndMatchAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _predictions.Verify(r => r.AddAsync(It.IsAny<Prediction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DrawPrediction_Succeeds()
    {
        var match = MakeMatch();
        var command = new SubmitPredictionCommand(match.Id, 1, 1);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByUserAndMatchAsync(_userId, match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Prediction?)null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PredictedHomeGoals.Should().Be(1);
        result.Value.PredictedAwayGoals.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ZeroZeroDraw_Succeeds()
    {
        var match = MakeMatch();
        var command = new SubmitPredictionCommand(match.Id, 0, 0);

        _matches.Setup(r => r.GetByIdAsync(match.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(match);
        _predictions.Setup(r => r.GetByUserAndMatchAsync(_userId, match.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Prediction?)null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PredictedHomeGoals.Should().Be(0);
        result.Value.PredictedAwayGoals.Should().Be(0);
    }
}
