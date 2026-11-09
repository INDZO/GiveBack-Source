using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Organizations.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Organizations.GetStatistics;

public sealed class GetStatisticsQueryHandler : IRequestHandler<GetStatisticsQuery, OrganizationStatisticsDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public GetStatisticsQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrganizationStatisticsDto> Handle(GetStatisticsQuery request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunityIds = _unitOfWork.Opportunities.Query().Where(o => o.OrganizationId == organization.Id).Select(o => o.Id);
        var acceptedApplications = await _unitOfWork.Applications.Query()
            .Where(a => opportunityIds.Contains(a.OpportunityId) && (a.Status == ApplicationStatus.Accepted || a.Status == ApplicationStatus.Attended))
            .Include(a => a.Team)
            .ThenInclude(t => t!.Members)
            .ToListAsync(cancellationToken);
        var volunteerIds = acceptedApplications
            .SelectMany(a => a.VolunteerId.HasValue
                ? new[] { a.VolunteerId.Value }
                : a.Team?.Members.Where(m => m.Status == TeamMemberStatus.Confirmed).Select(m => m.VolunteerId) ?? Enumerable.Empty<int>())
            .Distinct()
            .Count();

        return new OrganizationStatisticsDto
        {
            TotalPublishedOpportunities = await _unitOfWork.Opportunities.Query().CountAsync(o => o.OrganizationId == organization.Id, cancellationToken),
            TotalVolunteers = volunteerIds,
            TotalApprovedHours = await _unitOfWork.HourLogs.Query()
                .Where(h => h.Opportunity.OrganizationId == organization.Id && (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected))
                .SumAsync(h => h.ApprovedHours ?? 0, cancellationToken),
            TotalBeneficiariesHelped = await _unitOfWork.Repository<GiveBack.Domain.Entities.ImpactReport>().Query()
                .Where(r => r.Opportunity.OrganizationId == organization.Id)
                .SumAsync(r => r.BeneficiariesHelpedCount, cancellationToken),
            CompletedActionsCount = await _unitOfWork.Opportunities.Query()
                .CountAsync(o => o.OrganizationId == organization.Id && o.Status == OpportunityStatus.Completed, cancellationToken)
        };
    }
}
