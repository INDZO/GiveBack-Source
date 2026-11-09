using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.ImpactReports.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.ImpactReports.CreateImpactReport;

public sealed class CreateImpactReportCommandHandler : IRequestHandler<CreateImpactReportCommand, ImpactReportDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateImpactReportCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ImpactReportDto> Handle(CreateImpactReportCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.OpportunityId, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can create the impact report.");
        }
        if (opportunity.ImpactReport is not null)
        {
            throw new ConflictException("Impact report already exists for this opportunity.");
        }
        if (opportunity.Status != OpportunityStatus.Completed)
        {
            throw new ConflictException("Impact report can be created only for completed opportunities.");
        }
        var logs = _unitOfWork.HourLogs.Query()
            .Where(h => h.OpportunityId == opportunity.Id && (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected));
        if (!await logs.AnyAsync(cancellationToken))
        {
            throw new ConflictException("Impact report requires at least one verified hour log.");
        }
        var report = new ImpactReport
        {
            OpportunityId = opportunity.Id,
            VolunteersCount = await logs.Select(h => h.VolunteerId).Distinct().CountAsync(cancellationToken),
            TotalHours = await logs.SumAsync(h => h.ApprovedHours ?? 0, cancellationToken),
            BeneficiariesHelpedCount = request.BeneficiariesHelpedCount,
            Summary = request.Summary.Trim()
        };
        await _unitOfWork.Repository<ImpactReport>().AddAsync(report, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _unitOfWork.Repository<ImpactReport>().Query().Include(r => r.Opportunity).FirstAsync(r => r.Id == report.Id, cancellationToken);
        return _mapper.Map<ImpactReportDto>(saved);
    }
}
