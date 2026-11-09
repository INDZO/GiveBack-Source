using Asp.Versioning;
using GiveBack.Application.Features.Volunteers.SearchVolunteers;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = $"{nameof(UserRole.Organization)},{nameof(UserRole.Admin)}")]
[Route("api/v{version:apiVersion}/volunteers")]
public sealed class VolunteerSearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public VolunteerSearchController(IMediator mediator) => _mediator = mediator;

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] int? skillId, [FromQuery] int? causeId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new SearchVolunteersQuery(name, skillId, causeId), cancellationToken));
}
