using Asp.Versioning;
using GiveBack.Application.Features.Organizations.GetMyOpportunities;
using GiveBack.Application.Features.Organizations.GetStatistics;
using GiveBack.Application.Features.Organizations.UpdateMyProfile;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GetOrganizationProfileQuery = GiveBack.Application.Features.Organizations.GetMyProfile.GetMyProfileQuery;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = nameof(UserRole.Organization))]
[Route("api/v{version:apiVersion}/organizations")]
public sealed class OrganizationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrganizationsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOrganizationProfileQuery(), cancellationToken));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateMyProfileCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("me/statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetStatisticsQuery(), cancellationToken));

    [HttpGet("me/opportunities")]
    public async Task<IActionResult> GetMyOpportunities(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyOpportunitiesQuery(), cancellationToken));
}
