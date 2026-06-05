using FluentAssertions;
using Moq;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Leaderboard.DTOs;
using PollaMundialista.Application.Features.Leaderboard.Queries.GetLeaderboard;
using PollaMundialista.Domain.Entities;
using Match = PollaMundialista.Domain.Entities.Match;

namespace PollaMundialista.Tests.Application.Leaderboard;

public class GetLeaderboardHandlerTests
{
    private readonly Mock<IPredictionRepository> _predictions = new();

    private GetLeaderboardQueryHandler CreateHandler() => new(_predictions.Object);

    private static User MakeUser(string displayName)
        => User.Create($"{displayName.ToLower().Replace(" ", "")}@test.com", "hash", displayName);

    private static Match MakeFinishedMatch(int home, int away)
    {
        var match = Match.Create("Group A", "Home", "Away",
            new DateTime(2026, 6, 15, 18, 0, 0, DateTimeKind.Utc));
        match.SetResult(home, away);
        return match;
    }

    private static Prediction MakeScoredPrediction(User user, Match match, int predHome, int predAway, int points)
    {
        var prediction = Prediction.Create(user.Id, match.Id, predHome, predAway);
        prediction.AwardPoints(points);
        typeof(Prediction).GetProperty(nameof(Prediction.User))!.SetValue(prediction, user);
        return prediction;
    }

    [Fact]
    public async Task Handle_NoPredictions_ReturnsEmptyLeaderboard()
    {
        _predictions.Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>());

        var result = await CreateHandler().Handle(new GetLeaderboardQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleUsers_OrderedByTotalPointsDescending()
    {
        var match = MakeFinishedMatch(2, 1);
        var alice = MakeUser("Alice");
        var bob = MakeUser("Bob");
        var charlie = MakeUser("Charlie");

        _predictions.Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>
                    {
                        MakeScoredPrediction(bob, match, 1, 0, 1),
                        MakeScoredPrediction(charlie, match, 0, 2, 0),
                        MakeScoredPrediction(alice, match, 2, 1, 3),
                    });

        var result = await CreateHandler().Handle(new GetLeaderboardQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(3);
        result.Value[0].Should().Be(new LeaderboardEntryDto(alice.Id, 1, "Alice", 3, 1));
        result.Value[1].Should().Be(new LeaderboardEntryDto(bob.Id, 2, "Bob", 1, 0));
        result.Value[2].Should().Be(new LeaderboardEntryDto(charlie.Id, 3, "Charlie", 0, 0));
    }

    [Fact]
    public async Task Handle_TieOnPoints_TiebrokenByExactHits()
    {
        // Alice: 3 pts, 1 exact hit (one exact prediction)
        // Bob:   3 pts, 0 exact hits (three outcome-only predictions across three matches)
        var match1 = MakeFinishedMatch(2, 1);
        var match2 = MakeFinishedMatch(0, 0);
        var match3 = MakeFinishedMatch(1, 0);
        var alice = MakeUser("Alice");
        var bob = MakeUser("Bob");

        _predictions.Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>
                    {
                        MakeScoredPrediction(alice, match1, 2, 1, 3),  // exact → 3 pts, 1 exactHit
                        MakeScoredPrediction(bob, match1, 1, 0, 1),    // outcome only
                        MakeScoredPrediction(bob, match2, 0, 0, 1),    // exact draw BUT awarded as 1 for test
                        MakeScoredPrediction(bob, match3, 2, 1, 1),    // outcome only → total 3 pts, 0 exactHits
                    });

        var result = await CreateHandler().Handle(new GetLeaderboardQuery(), CancellationToken.None);

        result.Value![0].DisplayName.Should().Be("Alice");
        result.Value[0].TotalPoints.Should().Be(3);
        result.Value[0].ExactHits.Should().Be(1);
        result.Value[1].DisplayName.Should().Be("Bob");
        result.Value[1].TotalPoints.Should().Be(3);
        result.Value[1].ExactHits.Should().Be(0);
    }

    [Fact]
    public async Task Handle_EqualPointsAndExactHits_TiebrokenAlphabetically()
    {
        var match = MakeFinishedMatch(1, 0);
        var zara = MakeUser("Zara");
        var anna = MakeUser("Anna");

        _predictions.Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>
                    {
                        MakeScoredPrediction(zara, match, 2, 1, 1),
                        MakeScoredPrediction(anna, match, 3, 0, 1),
                    });

        var result = await CreateHandler().Handle(new GetLeaderboardQuery(), CancellationToken.None);

        result.Value![0].DisplayName.Should().Be("Anna");
        result.Value[1].DisplayName.Should().Be("Zara");
    }

    [Fact]
    public async Task Handle_UserWithMultipleMatches_SumsPointsCorrectly()
    {
        var match1 = MakeFinishedMatch(2, 1);
        var match2 = MakeFinishedMatch(0, 0);
        var alice = MakeUser("Alice");

        _predictions.Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Prediction>
                    {
                        MakeScoredPrediction(alice, match1, 2, 1, 3),
                        MakeScoredPrediction(alice, match2, 1, 1, 1),
                    });

        var result = await CreateHandler().Handle(new GetLeaderboardQuery(), CancellationToken.None);

        result.Value!.Should().HaveCount(1);
        result.Value[0].Should().Be(new LeaderboardEntryDto(alice.Id, 1, "Alice", 4, 1));
    }

    [Fact]
    public async Task Handle_RanksAreConsecutive_StartingAtOne()
    {
        var match = MakeFinishedMatch(1, 0);
        var users = new[] { MakeUser("C"), MakeUser("A"), MakeUser("B") };
        var points = new[] { 1, 3, 0 };

        var predList = users.Zip(points, (u, p) =>
            MakeScoredPrediction(u, match, 0, 0, p)).ToList();

        _predictions.Setup(r => r.GetAllWithUsersAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(predList);

        var result = await CreateHandler().Handle(new GetLeaderboardQuery(), CancellationToken.None);

        result.Value!.Select(e => e.Rank).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }
}
