using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Badges.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Badges.GetMyBadges;

public sealed class GetMyBadgesQueryHandler : IRequestHandler<GetMyBadgesQuery, IReadOnlyList<VolunteerBadgeDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyBadgesQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VolunteerBadgeDto>> Handle(GetMyBadgesQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        return await _unitOfWork.Repository<VolunteerBadge>().Query()
            .Where(vb => vb.VolunteerId == volunteer.Id)
            .OrderByDescending(vb => vb.AwardedAt)
            .ProjectTo<VolunteerBadgeDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
