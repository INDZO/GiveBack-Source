using Asp.Versioning;
using GiveBack.Application.Features.Teams.AddTeamMember;
using GiveBack.Application.Features.Teams.ConfirmTeamMember;
using GiveBack.Application.Features.Teams.CreateTeam;
using GiveBack.Application.Features.Teams.DeclineTeamMember;
using GiveBack.Application.Features.Teams.GetMyTeams;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = nameof(UserRole.Volunteer))]
[Route("api/v{version:apiVersion}/teams")]
public sealed class TeamsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeamsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTeamCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyTeamsQuery(), cancellationToken));

    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, AddMemberRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AddTeamMemberCommand(id, request.VolunteerId), cancellationToken));

    [HttpPatch("{id:int}/members/{memberId:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, int memberId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ConfirmTeamMemberCommand(id, memberId), cancellationToken));

    [HttpPatch("{id:int}/members/{memberId:int}/decline")]
    public async Task<IActionResult> Decline(int id, int memberId, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new DeclineTeamMemberCommand(id, memberId), cancellationToken));

    public sealed record AddMemberRequest(int VolunteerId);
}
