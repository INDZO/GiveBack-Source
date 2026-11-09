using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.GetUrgentOpportunities;

public sealed class GetUrgentOpportunitiesQueryHandler : IRequestHandler<GetUrgentOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUrgentOpportunitiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OpportunityDto>> Handle(GetUrgentOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await _unitOfWork.Opportunities.Query()
            .Where(o => o.IsUrgent &&
                        o.Status == OpportunityStatus.Open &&
                        o.Visibility == OpportunityVisibility.Public &&
                        (o.EndAt.HasValue ? o.EndAt.Value > now : o.StartAt.AddSeconds((double)(o.DurationHours * 3600)) > now))
            .OrderBy(o => o.StartAt)
            .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
