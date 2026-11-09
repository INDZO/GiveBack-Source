using Asp.Versioning;
using GiveBack.Application.Features.Opportunities.AcceptInvitation;
using GiveBack.Application.Features.Opportunities.CancelOpportunity;
using GiveBack.Application.Features.Opportunities.CompleteOpportunity;
using GiveBack.Application.Features.Opportunities.CreateOpportunity;
using GiveBack.Application.Features.Opportunities.DeclineInvitation;
using GiveBack.Application.Features.Opportunities.DeleteOpportunity;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Application.Features.Opportunities.GetMatchingOpportunities;
using GiveBack.Application.Features.Opportunities.GetMyInvitations;
using GiveBack.Application.Features.Opportunities.GetOpportunities;
using GiveBack.Application.Features.Opportunities.GetOpportunityById;
using GiveBack.Application.Features.Opportunities.GetUrgentOpportunities;
using GiveBack.Application.Features.Opportunities.InviteVolunteer;
using GiveBack.Application.Features.Opportunities.UpdateOpportunity;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GiveBack.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/opportunities")]
public sealed class OpportunitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public OpportunitiesController(IMediator mediator) => _mediator = mediator;

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPost]
    public async Task<IActionResult> Create(OpportunityWriteDto dto, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CreateOpportunityCommand(dto), cancellationToken));

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] int? causeId, [FromQuery] string? location, [FromQuery] DateTime? date, [FromQuery] DateTime? dateFrom, [FromQuery] DateTime? dateTo, [FromQuery] int[]? skillIds, [FromQuery] OpportunityFormat? format, [FromQuery] decimal? minDurationHours, [FromQuery] decimal? maxDurationHours, [FromQuery] OpportunityType? type, [FromQuery] bool? isUrgent, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sortBy = null, CancellationToken cancellationToken = default)
        => Ok(await _mediator.Send(new GetOpportunitiesQuery(search, causeId, location, date, dateFrom, dateTo, skillIds, format, minDurationHours, maxDurationHours, type, isUrgent, page, pageSize, sortBy), cancellationToken));

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOpportunityByIdQuery(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, OpportunityWriteDto dto, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new UpdateOpportunityCommand(id, dto), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteOpportunityCommand(id), cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CancelOpportunityCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPatch("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new CompleteOpportunityCommand(id), cancellationToken));

    [AllowAnonymous]
    [HttpGet("urgent")]
    public async Task<IActionResult> Urgent(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetUrgentOpportunitiesQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpGet("matching-me")]
    public async Task<IActionResult> MatchingMe(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMatchingOpportunitiesQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Organization))]
    [HttpPost("{id:int}/invite")]
    public async Task<IActionResult> Invite(int id, InviteVolunteerRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new InviteVolunteerCommand(id, request.VolunteerId), cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpGet("invitations/my")]
    public async Task<IActionResult> MyInvitations(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetMyInvitationsQuery(), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpPatch("invitations/{id:int}/accept")]
    public async Task<IActionResult> AcceptInvitation(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new AcceptInvitationCommand(id), cancellationToken));

    [Authorize(Roles = nameof(UserRole.Volunteer))]
    [HttpPatch("invitations/{id:int}/decline")]
    public async Task<IActionResult> DeclineInvitation(int id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new DeclineInvitationCommand(id), cancellationToken));

    public sealed record InviteVolunteerRequest(int VolunteerId);
}
