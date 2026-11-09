using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Admin.GetPendingOrganizations;

public sealed class GetPendingOrganizationsQueryHandler : IRequestHandler<GetPendingOrganizationsQuery, IReadOnlyList<OrganizationProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPendingOrganizationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OrganizationProfileDto>> Handle(GetPendingOrganizationsQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Organizations.Query()
            .Where(o => !o.IsVerified && o.VerificationRejectionReason == null && o.User.IsActive)
            .ProjectTo<OrganizationProfileDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
