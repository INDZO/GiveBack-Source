using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Badges.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Badges.GetBadges;

public sealed class GetBadgesQueryHandler : IRequestHandler<GetBadgesQuery, IReadOnlyList<BadgeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBadgesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BadgeDto>> Handle(GetBadgesQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Badge>().Query()
            .OrderBy(b => b.Id)
            .ProjectTo<BadgeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
