namespace PollaMundialista.Domain.Entities;

public class Match
{
    public Guid Id { get; private set; }
    public string GroupName { get; private set; } = default!;
    public string HomeTeam { get; private set; } = default!;
    public string AwayTeam { get; private set; } = default!;
    public DateTime MatchDate { get; private set; }
    public int? HomeGoals { get; private set; }
    public int? AwayGoals { get; private set; }
    public bool IsFinished { get; private set; }

    public ICollection<Prediction> Predictions { get; private set; } = new List<Prediction>();

    private Match() { }

    public static Match Create(string groupName, string homeTeam, string awayTeam, DateTime matchDate)
    {
        return new Match
        {
            Id = Guid.NewGuid(),
            GroupName = groupName,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            MatchDate = matchDate,
            IsFinished = false
        };
    }

    public void SetResult(int homeGoals, int awayGoals)
    {
        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        IsFinished = true;
    }
}
