using Asp.Versioning;
using GiveBack.Application.Features.Badges.GetBadges;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/badges")]
public sealed class BadgesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BadgesController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetBadgesQuery(), cancellationToken));
}
