using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Teams.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Teams.GetMyTeams;

public sealed class GetMyTeamsQueryHandler : IRequestHandler<GetMyTeamsQuery, IReadOnlyList<VolunteerTeamDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyTeamsQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VolunteerTeamDto>> Handle(GetMyTeamsQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        return await _unitOfWork.Repository<VolunteerTeam>().Query()
            .Where(t => t.CaptainVolunteerId == volunteer.Id ||
                        t.Members.Any(m => m.VolunteerId == volunteer.Id && m.Status != TeamMemberStatus.Declined))
            .OrderByDescending(t => t.CreatedAt)
            .ProjectTo<VolunteerTeamDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
