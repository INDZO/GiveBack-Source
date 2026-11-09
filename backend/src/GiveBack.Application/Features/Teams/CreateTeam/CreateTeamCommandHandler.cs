using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Teams.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Teams.CreateTeam;

public sealed class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, VolunteerTeamDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateTeamCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VolunteerTeamDto> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        var captain = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var team = new VolunteerTeam
        {
            CaptainVolunteerId = captain.Id,
            Name = request.Name.Trim(),
            Members = new List<VolunteerTeamMember>
            {
                new() { VolunteerId = captain.Id, Status = TeamMemberStatus.Confirmed }
            }
        };
        await _unitOfWork.Repository<VolunteerTeam>().AddAsync(team, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _unitOfWork.Repository<VolunteerTeam>().Query()
            .Include(t => t.Members)
            .ThenInclude(m => m.Volunteer)
            .FirstAsync(t => t.Id == team.Id, cancellationToken);
        return _mapper.Map<VolunteerTeamDto>(saved);
    }
}
