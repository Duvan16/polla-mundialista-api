using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Application.Features.Leaderboard.Queries.GetLeaderboard;
using PollaMundialista.Application.Features.Leaderboard.Queries.GetUserHistory;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaderboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboard(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLeaderboardQuery(), cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("users/{userId:guid}/history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserHistory(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserHistoryQuery(userId), cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Error == "Forbidden"
            ? StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error })
            : NotFound(new { error = result.Error });
    }
}
