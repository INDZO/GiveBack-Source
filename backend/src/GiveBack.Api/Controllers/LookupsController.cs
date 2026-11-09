using Asp.Versioning;
using GiveBack.Application.Features.Lookups.GetCauses;
using GiveBack.Application.Features.Lookups.GetSkills;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/lookups")]
public sealed class LookupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LookupsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("skills")]
    public async Task<IActionResult> Skills(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetSkillsQuery(), cancellationToken));

    [HttpGet("causes")]
    public async Task<IActionResult> Causes(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCausesQuery(), cancellationToken));
}
