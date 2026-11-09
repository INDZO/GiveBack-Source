using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.GetAdminOpportunities;

public sealed class GetAdminOpportunitiesQueryHandler : IRequestHandler<GetAdminOpportunitiesQuery, IReadOnlyList<OpportunityDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAdminOpportunitiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OpportunityDto>> Handle(GetAdminOpportunitiesQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Opportunities.Query()
            .OrderByDescending(o => o.CreatedAt)
            .ProjectTo<OpportunityDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
