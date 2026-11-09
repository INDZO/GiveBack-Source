using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Admin.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.GetAdminStatistics;

public sealed class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken cancellationToken)
    {
        return new AdminStatisticsDto
        {
            UsersCount = await _unitOfWork.Users.Query().CountAsync(cancellationToken),
            VolunteersCount = await _unitOfWork.Volunteers.Query().CountAsync(cancellationToken),
            OrganizationsCount = await _unitOfWork.Organizations.Query().CountAsync(cancellationToken),
            VerifiedOrganizationsCount = await _unitOfWork.Organizations.Query().CountAsync(o => o.IsVerified, cancellationToken),
            PendingOrganizationsCount = await _unitOfWork.Organizations.Query().CountAsync(o => !o.IsVerified && o.VerificationRejectionReason == null && o.User.IsActive, cancellationToken),
            OpportunitiesCount = await _unitOfWork.Opportunities.Query().CountAsync(cancellationToken),
            OpenOpportunitiesCount = await _unitOfWork.Opportunities.Query().CountAsync(o => o.Status == OpportunityStatus.Open, cancellationToken),
            FilledOpportunitiesCount = await _unitOfWork.Opportunities.Query().CountAsync(o => o.Status == OpportunityStatus.Filled, cancellationToken),
            CancelledOpportunitiesCount = await _unitOfWork.Opportunities.Query().CountAsync(o => o.Status == OpportunityStatus.Cancelled, cancellationToken),
            CompletedOpportunitiesCount = await _unitOfWork.Opportunities.Query().CountAsync(o => o.Status == OpportunityStatus.Completed, cancellationToken),
            UrgentOpportunitiesCount = await _unitOfWork.Opportunities.Query().CountAsync(o => o.IsUrgent, cancellationToken),
            ApplicationsCount = await _unitOfWork.Applications.Query().CountAsync(cancellationToken),
            TotalVerifiedHours = await _unitOfWork.HourLogs.Query()
                .Where(h => h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected)
                .SumAsync(h => h.ApprovedHours ?? 0, cancellationToken),
            CertificatesCount = await _unitOfWork.Repository<Certificate>().Query().CountAsync(cancellationToken)
        };
    }
}
