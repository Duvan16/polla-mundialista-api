using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.Queries.GetUpcomingMatches;
using PollaMundialista.Domain.Entities;
using Match = PollaMundialista.Domain.Entities.Match;

namespace PollaMundialista.Tests.Application.Predictions;

public class GetUpcomingMatchesHandlerTests
{
    private readonly Mock<IMatchRepository> _matches = new();
    private readonly Mock<IPredictionRepository> _predictions = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private readonly Guid _userId = Guid.NewGuid();

    private GetUpcomingMatchesQueryHandler CreateHandler() =>
        new(_matches.Object, _predictions.Object, _currentUser.Object);

    private static Match MakeMatch(string home, string away, bool finished = false)
    {
        var m = Match.Create("Group A", home, away,
            new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc));
        if (finished) m.SetResult(1, 0);
        return m;
    }

    [Fact]
    public async Task Handle_AuthenticatedUser_ReturnsOnlyUpcomingWithPrediction()
    {
        var upcoming = MakeMatch("A", "B");
        var finished = MakeMatch("C", "D", finished: true);

        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(_userId);

        _matches.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Match> { upcoming, finished });

        var pred = Prediction.Create(_userId, upcoming.Id, 2, 1);
        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { pred });

        var result = await CreateHandler().Handle(new GetUpcomingMatchesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);

        var dto = result.Value[0];
        dto.MatchId.Should().Be(upcoming.Id);
        dto.MyPredictedHomeGoals.Should().Be(2);
        dto.MyPredictedAwayGoals.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AuthenticatedUser_NoPrediction_ReturnsNullGoals()
    {
        var upcoming = MakeMatch("A", "B");

        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(_userId);

        _matches.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Match> { upcoming });
        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>());

        var result = await CreateHandler().Handle(new GetUpcomingMatchesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value![0];
        dto.MyPredictedHomeGoals.Should().BeNull();
        dto.MyPredictedAwayGoals.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AnonymousUser_DoesNotQueryPredictions()
    {
        var upcoming = MakeMatch("A", "B");

        _currentUser.Setup(u => u.IsAuthenticated).Returns(false);

        _matches.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Match> { upcoming });

        var result = await CreateHandler().Handle(new GetUpcomingMatchesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value[0].MyPredictedHomeGoals.Should().BeNull();

        _predictions.Verify(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AllMatchesFinished_ReturnsEmptyList()
    {
        var finished1 = MakeMatch("A", "B", finished: true);
        var finished2 = MakeMatch("C", "D", finished: true);

        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(_userId);

        _matches.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Match> { finished1, finished2 });
        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>());

        var result = await CreateHandler().Handle(new GetUpcomingMatchesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleUpcoming_ReturnsAll()
    {
        var m1 = MakeMatch("A", "B");
        var m2 = MakeMatch("C", "D");
        var m3 = MakeMatch("E", "F");

        _currentUser.Setup(u => u.IsAuthenticated).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(_userId);

        _matches.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Match> { m1, m2, m3 });
        _predictions.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>());

        var result = await CreateHandler().Handle(new GetUpcomingMatchesQuery(), CancellationToken.None);

        result.Value!.Should().HaveCount(3);
    }
}
