using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Organizations.GetMyOpportunities;

public sealed class GetMyOpportunitiesQueryHandler : IRequestHandler<GetMyOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyOpportunitiesQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OpportunityDto>> Handle(GetMyOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");

        var dtos = await _unitOfWork.Opportunities.Query()
            .Where(o => o.OrganizationId == organization.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        if (dtos.Count == 0)
        {
            return dtos;
        }

        // Compute management capability flags from related data, so the UI can
        // show/hide actions authoritatively instead of guessing backend rules.
        var oppIds = dtos.Select(d => d.Id).ToList();

        var reportedIds = (await _unitOfWork.Repository<ImpactReport>().Query()
            .Where(r => oppIds.Contains(r.OpportunityId))
            .Select(r => r.OpportunityId)
            .ToListAsync(cancellationToken)).ToHashSet();

        var verifiedHourCounts = (await _unitOfWork.HourLogs.Query()
            .Where(h => oppIds.Contains(h.OpportunityId) &&
                        (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected))
            .GroupBy(h => h.OpportunityId)
            .Select(g => new { OpportunityId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken))
            .ToDictionary(x => x.OpportunityId, x => x.Count);

        foreach (var dto in dtos)
        {
            var open = dto.Status == OpportunityStatus.Open;
            var openOrFilled = open || dto.Status == OpportunityStatus.Filled;
            var completed = dto.Status == OpportunityStatus.Completed;
            var verifiedCount = verifiedHourCounts.TryGetValue(dto.Id, out var c) ? c : 0;
            var hasReport = reportedIds.Contains(dto.Id);

            dto.VerifiedHoursCount = verifiedCount;
            dto.HasImpactReport = hasReport;
            dto.CanEdit = openOrFilled;
            dto.CanDelete = openOrFilled;
            dto.CanComplete = openOrFilled;
            dto.CanCancel = openOrFilled;
            // Invite mirrors ApplyTeam/Invite rules: verified org + private + open.
            dto.CanInvite = organization.IsVerified
                && dto.Visibility == OpportunityVisibility.Private
                && open;
            // Mirrors CreateImpactReportCommandHandler: completed, no existing
            // report, and at least one verified hour log.
            dto.CanCreateImpactReport = completed && !hasReport && verifiedCount > 0;
        }

        return dtos;
    }
}
