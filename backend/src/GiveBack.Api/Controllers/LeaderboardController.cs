using Asp.Versioning;
using GiveBack.Application.Features.Leaderboard.GetMonthlyLeaderboard;
using GiveBack.Application.Features.Leaderboard.GetYearlyLeaderboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/leaderboard")]
public sealed class LeaderboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeaderboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int month, [FromQuery] int year, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMonthlyLeaderboardQuery(month, year), cancellationToken));

    [HttpGet("yearly")]
    public async Task<IActionResult> Yearly([FromQuery] int year, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetYearlyLeaderboardQuery(year), cancellationToken));
}
