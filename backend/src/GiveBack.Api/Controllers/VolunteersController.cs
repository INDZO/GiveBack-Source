using Asp.Versioning;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Application.Features.Volunteers.GetAnnualReport;
using GiveBack.Application.Features.Volunteers.GetMyBadges;
using GiveBack.Application.Features.Volunteers.GetMyCertificates;
using GiveBack.Application.Features.Volunteers.GetMyHours;
using GiveBack.Application.Features.Volunteers.UpdateAvailability;
using GiveBack.Application.Features.Volunteers.UpdateInterests;
using GiveBack.Application.Features.Volunteers.UpdateMyProfile;
using GiveBack.Application.Features.Volunteers.UpdateSkills;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetVolunteerProfileQuery = GiveBack.Application.Features.Volunteers.GetMyProfile.GetMyProfileQuery;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = nameof(UserRole.Volunteer))]
[Route("api/v{version:apiVersion}/volunteers")]
public sealed class VolunteersController : ControllerBase
{
    private readonly IMediator _mediator;

    public VolunteersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetVolunteerProfileQuery(), cancellationToken));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateMyProfileCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPut("me/skills")]
    public async Task<IActionResult> UpdateSkills(UpdateSkillsCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPut("me/interests")]
    public async Task<IActionResult> UpdateInterests(UpdateInterestsCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPut("me/availability")]
    public async Task<IActionResult> UpdateAvailability(UpdateAvailabilityCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("me/hours")]
    public async Task<IActionResult> GetHours(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyHoursQuery(), cancellationToken));

    [HttpGet("me/badges")]
    public async Task<IActionResult> GetBadges(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyBadgesQuery(), cancellationToken));

    [HttpGet("me/certificates")]
    public async Task<IActionResult> GetCertificates(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyCertificatesQuery(), cancellationToken));

    [HttpGet("me/annual-report")]
    public async Task<IActionResult> GetAnnualReport([FromQuery] int year, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetAnnualReportQuery(year), cancellationToken));
}
