using Asp.Versioning;
using GiveBack.Application.Features.Notifications.GetNotifications;
using GiveBack.Application.Features.Notifications.MarkAllNotificationsRead;
using GiveBack.Application.Features.Notifications.MarkNotificationRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetNotificationsQuery(), cancellationToken));

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkNotificationReadCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }
}
