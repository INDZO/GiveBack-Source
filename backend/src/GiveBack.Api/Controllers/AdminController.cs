using Asp.Versioning;
using GiveBack.Application.Features.Admin.ActivateUser;
using GiveBack.Application.Features.Admin.CancelOpportunityAsAdmin;
using GiveBack.Application.Features.Admin.CreateCause;
using GiveBack.Application.Features.Admin.DeactivateUser;
using GiveBack.Application.Features.Admin.DeleteCause;
using GiveBack.Application.Features.Admin.GetAdminCauses;
using GiveBack.Application.Features.Admin.GetAdminOpportunities;
using GiveBack.Application.Features.Admin.GetAdminStatistics;
using GiveBack.Application.Features.Admin.GetPendingOrganizations;
using GiveBack.Application.Features.Admin.GetUsers;
using GiveBack.Application.Features.Admin.RejectOrganization;
using GiveBack.Application.Features.Admin.UpdateCause;
using GiveBack.Application.Features.Admin.VerifyOrganization;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = nameof(UserRole.Admin))]
[Route("api/v{version:apiVersion}/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("organizations/pending")]
    public async Task<IActionResult> GetPendingOrganizations(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetPendingOrganizationsQuery(), cancellationToken));

    [HttpPatch("organizations/{id:int}/verify")]
    public async Task<IActionResult> VerifyOrganization(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new VerifyOrganizationCommand(id), cancellationToken));

    [HttpPatch("organizations/{id:int}/reject")]
    public async Task<IActionResult> RejectOrganization(int id, RejectOrganizationRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RejectOrganizationCommand(id, request.Reason), cancellationToken));

    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAdminStatisticsQuery(), cancellationToken));

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetUsersQuery(), cancellationToken));

    [HttpPatch("users/{id:int}/activate")]
    public async Task<IActionResult> ActivateUser(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ActivateUserCommand(id), cancellationToken));

    [HttpPatch("users/{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateUser(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new DeactivateUserCommand(id), cancellationToken));

    [HttpGet("causes")]
    public async Task<IActionResult> Causes(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAdminCausesQuery(), cancellationToken));

    [HttpPost("causes")]
    public async Task<IActionResult> CreateCause(CreateCauseCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPut("causes/{id:int}")]
    public async Task<IActionResult> UpdateCause(int id, UpdateCauseRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new UpdateCauseCommand(id, request.Name), cancellationToken));

    [HttpDelete("causes/{id:int}")]
    public async Task<IActionResult> DeleteCause(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCauseCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("opportunities")]
    public async Task<IActionResult> Opportunities(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAdminOpportunitiesQuery(), cancellationToken));

    [HttpPatch("opportunities/{id:int}/cancel")]
    public async Task<IActionResult> CancelOpportunity(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CancelOpportunityAsAdminCommand(id), cancellationToken));

    public sealed record UpdateCauseRequest(string Name);
    public sealed record RejectOrganizationRequest(string Reason);
}
