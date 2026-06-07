using MediatR;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Features.Admin.Commands.SetMatchResult;

/// <summary>
/// Records the final score of a match and triggers point recalculation for all predictions on that match.
/// Restricted to Admin role.
/// </summary>
public record SetMatchResultCommand(Guid MatchId, int HomeGoals, int AwayGoals) : IRequest<Result>;
