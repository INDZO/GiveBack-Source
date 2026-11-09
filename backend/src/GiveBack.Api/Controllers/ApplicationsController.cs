using Asp.Versioning;
using GiveBack.Application.Features.Applications.AcceptApplication;
using GiveBack.Application.Features.Applications.ApplyTeamToOpportunity;
using GiveBack.Application.Features.Applications.ApplyToOpportunity;
using GiveBack.Application.Features.Applications.GetMyApplications;
using GiveBack.Application.Features.Applications.GetOrganizationApplications;
using GiveBack.Application.Features.Applications.MarkApplicationAttended;
using GiveBack.Application.Features.Applications.MarkApplicationNoShow;
using GiveBack.Application.Features.Applications.RejectApplication;
using GiveBack.Application.Features.Applications.WithdrawApplication;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpPost("opportunities/{id:int}/apply")]
    public async Task<IActionResult> Apply(int id, ApplyRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ApplyToOpportunityCommand(id, request.Message), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpPost("opportunities/{id:int}/apply-team")]
    public async Task<IActionResult> ApplyTeam(int id, ApplyTeamRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ApplyTeamToOpportunityCommand(id, request.TeamId, request.Message), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpGet("applications/my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyApplicationsQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpPatch("applications/{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new WithdrawApplicationCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpGet("organizations/applications")]
    public async Task<IActionResult> GetOrganizationApplications(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOrganizationApplicationsQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("applications/{id:int}/accept")]
    public async Task<IActionResult> Accept(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AcceptApplicationCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("applications/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RejectApplicationCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("applications/{id:int}/attended")]
    public async Task<IActionResult> Attended(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new MarkApplicationAttendedCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("applications/{id:int}/no-show")]
    public async Task<IActionResult> NoShow(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new MarkApplicationNoShowCommand(id), cancellationToken));

    public sealed record ApplyRequest(string? Message);
    public sealed record ApplyTeamRequest(int TeamId, string? Message);
}
