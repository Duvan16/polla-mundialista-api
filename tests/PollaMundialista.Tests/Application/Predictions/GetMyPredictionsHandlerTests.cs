using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.Queries.GetMyPredictions;
using PollaMundialista.Domain.Entities;
using Match = PollaMundialista.Domain.Entities.Match;

namespace PollaMundialista.Tests.Application.Predictions;

public class GetMyPredictionsHandlerTests
{
    private readonly Mock<IPredictionRepository> _predictions = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly Guid _userId = Guid.NewGuid();

    public GetMyPredictionsHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns(_userId);
    }

    private GetMyPredictionsQueryHandler CreateHandler() =>
        new(_predictions.Object, _currentUser.Object);

    private static (Match match, Prediction prediction) MakePair(
        Guid userId, bool finished = false, int? pointsAwarded = null)
    {
        var match = Match.Create("Group A", "Spain", "Portugal",
            new DateTime(2026, 6, 16, 18, 0, 0, DateTimeKind.Utc));

        if (finished) match.SetResult(2, 1);

        var pred = Prediction.Create(userId, match.Id, 1, 0);

        if (pointsAwarded.HasValue) pred.AwardPoints(pointsAwarded.Value);

        // Wire up navigation property via reflection (EF does this at runtime)
        typeof(Prediction)
            .GetProperty(nameof(Prediction.Match))!
            .SetValue(pred, match);

        return (match, pred);
    }

    [Fact]
    public async Task Handle_NoPredictions_ReturnsEmptyList()
    {
        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>());

        var result = await CreateHandler().Handle(new GetMyPredictionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FinishedMatchWithPoints_MapsCorrectly()
    {
        var (match, pred) = MakePair(_userId, finished: true, pointsAwarded: 3);

        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { pred });

        var result = await CreateHandler().Handle(new GetMyPredictionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value![0];
        dto.PredictionId.Should().Be(pred.Id);
        dto.MatchId.Should().Be(match.Id);
        dto.HomeTeam.Should().Be("Spain");
        dto.AwayTeam.Should().Be("Portugal");
        dto.PredictedHomeGoals.Should().Be(1);
        dto.PredictedAwayGoals.Should().Be(0);
        dto.ActualHomeGoals.Should().Be(2);
        dto.ActualAwayGoals.Should().Be(1);
        dto.IsFinished.Should().BeTrue();
        dto.PointsAwarded.Should().Be(3);
    }

    [Fact]
    public async Task Handle_UpcomingMatchNoPredictionPoints_PointsAwardedNull()
    {
        var (match, pred) = MakePair(_userId, finished: false);

        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { pred });

        var result = await CreateHandler().Handle(new GetMyPredictionsQuery(), CancellationToken.None);

        var dto = result.Value![0];
        dto.IsFinished.Should().BeFalse();
        dto.PointsAwarded.Should().BeNull();
        dto.ActualHomeGoals.Should().BeNull();
        dto.ActualAwayGoals.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MultipleMatches_ReturnsAll()
    {
        var (_, pred1) = MakePair(_userId, finished: true, pointsAwarded: 1);
        var (_, pred2) = MakePair(_userId, finished: false);

        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { pred1, pred2 });

        var result = await CreateHandler().Handle(new GetMyPredictionsQuery(), CancellationToken.None);

        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_MatchWithZeroPoints_MapsCorrectly()
    {
        var (_, pred) = MakePair(_userId, finished: true, pointsAwarded: 0);

        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { pred });

        var result = await CreateHandler().Handle(new GetMyPredictionsQuery(), CancellationToken.None);

        result.Value![0].PointsAwarded.Should().Be(0);
    }
}
