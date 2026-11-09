using Asp.Versioning;
using GiveBack.Application.Features.HourLogs.ApproveHourLog;
using GiveBack.Application.Features.HourLogs.CorrectHourLog;
using GiveBack.Application.Features.HourLogs.CreateHourLog;
using GiveBack.Application.Features.HourLogs.GetMyHourLogs;
using GiveBack.Application.Features.HourLogs.GetPendingOrganizationHourLogs;
using GiveBack.Application.Features.HourLogs.RejectHourLog;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class HourLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HourLogsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpPost("hour-logs")]
    public async Task<IActionResult> Create(CreateHourLogCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpGet("hour-logs/my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyHourLogsQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpGet("organizations/hour-logs/pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetPendingOrganizationHourLogsQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("hour-logs/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new ApproveHourLogCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("hour-logs/{id:int}/correct")]
    public async Task<IActionResult> Correct(int id, CorrectRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CorrectHourLogCommand(id, request.ApprovedHours), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("hour-logs/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new RejectHourLogCommand(id), cancellationToken));

    public sealed record CorrectRequest(decimal ApprovedHours);
}
