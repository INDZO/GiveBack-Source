using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Organizations.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.ImpactReports.GetOrganizationImpact;

public sealed class GetOrganizationImpactQueryHandler : IRequestHandler<GetOrganizationImpactQuery, OrganizationImpactDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public GetOrganizationImpactQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrganizationImpactDto> Handle(GetOrganizationImpactQuery request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var reports = _unitOfWork.Repository<ImpactReport>().Query().Where(r => r.Opportunity.OrganizationId == organization.Id);
        return new OrganizationImpactDto
        {
            ReportsCount = await reports.CountAsync(cancellationToken),
            TotalHours = await reports.SumAsync(r => r.TotalHours, cancellationToken),
            VolunteersCount = await reports.SumAsync(r => r.VolunteersCount, cancellationToken),
            BeneficiariesHelpedCount = await reports.SumAsync(r => r.BeneficiariesHelpedCount, cancellationToken)
        };
    }
}
