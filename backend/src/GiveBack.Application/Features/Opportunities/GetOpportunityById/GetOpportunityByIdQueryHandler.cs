using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.GetOpportunityById;

public sealed class GetOpportunityByIdQueryHandler : IRequestHandler<GetOpportunityByIdQuery, OpportunityDetailsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOpportunityAccessPolicy _accessPolicy;
    private readonly IMapper _mapper;

    public GetOpportunityByIdQueryHandler(IUnitOfWork unitOfWork, IOpportunityAccessPolicy accessPolicy, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _accessPolicy = accessPolicy;
        _mapper = mapper;
    }

    public async Task<OpportunityDetailsDto> Handle(GetOpportunityByIdQuery request, CancellationToken cancellationToken)
    {
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");

        if (opportunity.Visibility == OpportunityVisibility.Private &&
            !await _accessPolicy.CanViewAsync(opportunity, cancellationToken))
        {
            throw new NotFoundException("Opportunity was not found.");
        }

        return _mapper.Map<OpportunityDetailsDto>(opportunity);
    }
}
