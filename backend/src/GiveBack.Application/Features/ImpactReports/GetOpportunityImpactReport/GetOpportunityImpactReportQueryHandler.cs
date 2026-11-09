using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.ImpactReports.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.ImpactReports.GetOpportunityImpactReport;

public sealed class GetOpportunityImpactReportQueryHandler : IRequestHandler<GetOpportunityImpactReportQuery, ImpactReportDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOpportunityAccessPolicy _accessPolicy;
    private readonly IMapper _mapper;

    public GetOpportunityImpactReportQueryHandler(IUnitOfWork unitOfWork, IOpportunityAccessPolicy accessPolicy, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _accessPolicy = accessPolicy;
        _mapper = mapper;
    }

    public async Task<ImpactReportDto> Handle(GetOpportunityImpactReportQuery request, CancellationToken cancellationToken)
    {
        var report = await _unitOfWork.Repository<ImpactReport>().Query()
            .Include(r => r.Opportunity).ThenInclude(o => o.Organization)
            .Include(r => r.Opportunity).ThenInclude(o => o.Cause)
            .Include(r => r.Opportunity).ThenInclude(o => o.Invitations)
            .Include(r => r.Opportunity).ThenInclude(o => o.Applications).ThenInclude(a => a.Team).ThenInclude(t => t!.Members)
            .Where(r => r.OpportunityId == request.OpportunityId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Impact report was not found.");

        if (report.Opportunity.Visibility == OpportunityVisibility.Private &&
            !await _accessPolicy.CanViewAsync(report.Opportunity, cancellationToken))
        {
            throw new NotFoundException("Impact report was not found.");
        }

        return _mapper.Map<ImpactReportDto>(report);
    }
}
