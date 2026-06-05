using MediatR;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Common.Interfaces;
using PollaMundialista.Application.Features.Predictions.DTOs;

namespace PollaMundialista.Application.Features.Admin.Queries.GetAllMatchesAdmin;

public class GetAllMatchesAdminQueryHandler
    : IRequestHandler<GetAllMatchesAdminQuery, Result<IReadOnlyList<MatchWithPredictionDto>>>
{
    private readonly IMatchRepository _matches;

    public GetAllMatchesAdminQueryHandler(IMatchRepository matches) => _matches = matches;

    public async Task<Result<IReadOnlyList<MatchWithPredictionDto>>> Handle(
        GetAllMatchesAdminQuery request,
        CancellationToken cancellationToken)
    {
        var matches = await _matches.GetAllAsync(cancellationToken);

        var dtos = matches
            .OrderBy(m => m.MatchDate)
            .Select(m => new MatchWithPredictionDto(
                m.Id, m.GroupName, m.HomeTeam, m.AwayTeam, m.MatchDate,
                null, null, m.IsFinished, m.HomeGoals, m.AwayGoals))
            .ToList();

        return Result<IReadOnlyList<MatchWithPredictionDto>>.Success(dtos);
    }
}
