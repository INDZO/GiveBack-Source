using Asp.Versioning;
using GiveBack.Application.Features.Auth.Dtos;
using GiveBack.Application.Features.Auth.Login;
using GiveBack.Application.Features.Auth.Logout;
using GiveBack.Application.Features.Auth.RefreshToken;
using GiveBack.Application.Features.Auth.RegisterOrganization;
using GiveBack.Application.Features.Auth.RegisterVolunteer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [AllowAnonymous]
    [HttpPost("register/volunteer")]
    public async Task<ActionResult<AuthResponseDto>> RegisterVolunteer(RegisterVolunteerCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [HttpPost("register/organization")]
    public async Task<ActionResult<AuthResponseDto>> RegisterOrganization(RegisterOrganizationCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
