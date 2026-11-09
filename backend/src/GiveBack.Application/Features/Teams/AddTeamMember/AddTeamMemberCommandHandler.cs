using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Teams.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Teams.AddTeamMember;

public sealed class AddTeamMemberCommandHandler : IRequestHandler<AddTeamMemberCommand, TeamMemberDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddTeamMemberCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TeamMemberDto> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var captain = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var team = await _unitOfWork.Repository<VolunteerTeam>().Query()
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team was not found.");
        if (team.CaptainVolunteerId != captain.Id)
        {
            throw new ForbiddenException("Only the captain can add team members.");
        }
        var hasActiveApplications = await _unitOfWork.Applications.Query().AnyAsync(a =>
            a.TeamId == team.Id &&
            (a.Status == ApplicationStatus.Submitted ||
             a.Status == ApplicationStatus.Accepted ||
             a.Status == ApplicationStatus.Attended),
            cancellationToken);
        if (hasActiveApplications)
        {
            throw new ConflictException("Team membership cannot be changed while the team has active applications.");
        }
        _ = await _unitOfWork.Volunteers.GetDetailsAsync(request.VolunteerId, cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        if (team.Members.Any(m => m.VolunteerId == request.VolunteerId))
        {
            throw new ConflictException("Duplicate team membership is not allowed.");
        }
        var member = new VolunteerTeamMember { TeamId = team.Id, VolunteerId = request.VolunteerId, Status = TeamMemberStatus.Pending };
        team.Members.Add(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _unitOfWork.Repository<VolunteerTeamMember>().Query()
            .Include(m => m.Volunteer)
            .FirstAsync(m => m.Id == member.Id, cancellationToken);
        return _mapper.Map<TeamMemberDto>(saved);
    }
}
