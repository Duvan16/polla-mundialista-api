using MediatR;
using PollaMundialista.Application.Common;

namespace PollaMundialista.Application.Features.Admin.Commands.SetMatchResult;

public record SetMatchResultCommand(Guid MatchId, int HomeGoals, int AwayGoals) : IRequest<Result>;
