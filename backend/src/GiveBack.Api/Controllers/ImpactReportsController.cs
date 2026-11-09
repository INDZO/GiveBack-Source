using Asp.Versioning;
using GiveBack.Application.Features.ImpactReports.CreateImpactReport;
using GiveBack.Application.Features.ImpactReports.GetOpportunityImpactReport;
using GiveBack.Application.Features.ImpactReports.GetOrganizationImpact;
using GiveBack.Application.Features.ImpactReports.GetVolunteerAnnualImpact;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
public sealed class ImpactReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImpactReportsController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPost("opportunities/{id:int}/impact-report")]
    public async Task<IActionResult> Create(int id, CreateImpactReportRequest request, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CreateImpactReportCommand(id, request.BeneficiariesHelpedCount, request.Summary), cancellationToken));

    [AllowAnonymous]
    [HttpGet("opportunities/{id:int}/impact-report")]
    public async Task<IActionResult> GetOpportunityReport(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOpportunityImpactReportQuery(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpGet("organizations/me/impact")]
    public async Task<IActionResult> GetOrganizationImpact(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOrganizationImpactQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpGet("volunteers/me/annual-impact")]
    public async Task<IActionResult> GetVolunteerAnnualImpact([FromQuery] int year, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetVolunteerAnnualImpactQuery(year), cancellationToken));

    public sealed record CreateImpactReportRequest(int BeneficiariesHelpedCount, string Summary);
}
