using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Application.Features.Admin.Commands.SetMatchResult;
using PollaMundialista.Application.Features.Admin.Queries.GetAllMatchesAdmin;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("matches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMatches(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllMatchesAdminQuery(), cancellationToken);
        return Ok(result.Value);
    }

    [HttpPut("matches/{matchId:guid}/result")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetMatchResult(
        Guid matchId,
        [FromBody] SetMatchResultRequest body,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SetMatchResultCommand(matchId, body.HomeGoals, body.AwayGoals),
            cancellationToken);

        if (!result.IsSuccess)
            return result.Error == "Match not found."
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return NoContent();
    }
}

public record SetMatchResultRequest(int HomeGoals, int AwayGoals);
