using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Leaderboard.Queries.GetUserHistory;
using PollaMundialista.Domain.Entities;
using Match = PollaMundialista.Domain.Entities.Match;

namespace PollaMundialista.Tests.Application.Leaderboard;

public class GetUserHistoryHandlerTests
{
    private readonly Mock<IPredictionRepository> _predictions = new();
    private readonly Mock<IUserRepository> _users = new();

    private GetUserHistoryQueryHandler CreateHandler() =>
        new(_predictions.Object, _users.Object);

    private static User MakeUser()
        => User.Create("player@test.com", "hash", "Player One");

    private static Match MakeFinishedMatch(string home, string away, int homeGoals, int awayGoals)
    {
        var match = Match.Create("Group A", home, away,
            new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc));
        match.SetResult(homeGoals, awayGoals);
        return match;
    }

    private static Match MakeUpcomingMatch()
        => Match.Create("Group B", "Spain", "Germany",
            new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc));

    private static Prediction MakePrediction(Guid userId, Match match, int predHome, int predAway, int? points = null)
    {
        var p = Prediction.Create(userId, match.Id, predHome, predAway);
        if (points.HasValue) p.AwardPoints(points.Value);
        typeof(Prediction).GetProperty(nameof(Prediction.Match))!.SetValue(p, match);
        return p;
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var result = await CreateHandler().Handle(
            new GetUserHistoryQuery(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found.");
    }

    [Fact]
    public async Task Handle_UserWithNoFinishedPredictions_ReturnsEmptyList()
    {
        var user = MakeUser();
        var upcoming = MakeUpcomingMatch();
        var prediction = MakePrediction(user.Id, upcoming, 1, 0);

        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _predictions.Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { prediction });

        var result = await CreateHandler().Handle(
            new GetUserHistoryQuery(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserWithFinishedPredictions_ReturnsHistoryWithMatchDetails()
    {
        var user = MakeUser();
        var match = MakeFinishedMatch("Argentina", "Brazil", 2, 1);
        var prediction = MakePrediction(user.Id, match, 2, 1, 3);

        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _predictions.Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction> { prediction });

        var result = await CreateHandler().Handle(
            new GetUserHistoryQuery(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);

        var item = result.Value[0];
        item.HomeTeam.Should().Be("Argentina");
        item.AwayTeam.Should().Be("Brazil");
        item.PredictedHomeGoals.Should().Be(2);
        item.PredictedAwayGoals.Should().Be(1);
        item.ActualHomeGoals.Should().Be(2);
        item.ActualAwayGoals.Should().Be(1);
        item.PointsAwarded.Should().Be(3);
        item.IsFinished.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MixedFinishedAndUpcoming_OnlyFinishedMatchesReturned()
    {
        var user = MakeUser();
        var finished = MakeFinishedMatch("France", "Germany", 1, 0);
        var upcoming = MakeUpcomingMatch();

        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _predictions.Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>
                    {
                        MakePrediction(user.Id, finished, 1, 0, 3),
                        MakePrediction(user.Id, upcoming, 2, 1),
                    });

        var result = await CreateHandler().Handle(
            new GetUserHistoryQuery(user.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value[0].HomeTeam.Should().Be("France");
    }

    [Fact]
    public async Task Handle_MultipleFinishedMatches_OrderedByMatchDateDescending()
    {
        var user = MakeUser();

        var older = Match.Create("Group A", "A", "B", new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc));
        older.SetResult(1, 0);
        var newer = Match.Create("Group A", "C", "D", new DateTime(2026, 6, 20, 18, 0, 0, DateTimeKind.Utc));
        newer.SetResult(2, 2);

        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);
        _predictions.Setup(r => r.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>
                    {
                        MakePrediction(user.Id, older, 1, 0, 3),
                        MakePrediction(user.Id, newer, 2, 2, 3),
                    });

        var result = await CreateHandler().Handle(
            new GetUserHistoryQuery(user.Id), CancellationToken.None);

        result.Value![0].HomeTeam.Should().Be("C"); // newer match first
        result.Value[1].HomeTeam.Should().Be("A");
    }
}
