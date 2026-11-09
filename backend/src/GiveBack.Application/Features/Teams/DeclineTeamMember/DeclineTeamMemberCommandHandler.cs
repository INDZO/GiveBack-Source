using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Teams.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Teams.DeclineTeamMember;

public sealed class DeclineTeamMemberCommandHandler : IRequestHandler<DeclineTeamMemberCommand, TeamMemberDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeclineTeamMemberCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<TeamMemberDto> Handle(DeclineTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var member = await _unitOfWork.Repository<VolunteerTeamMember>().Query()
            .Include(m => m.Volunteer)
            .FirstOrDefaultAsync(m => m.Id == request.MemberId && m.TeamId == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team member was not found.");
        if (member.VolunteerId != volunteer.Id)
        {
            throw new ForbiddenException("Only the invited volunteer can update this membership.");
        }
        if (member.Status != TeamMemberStatus.Pending)
        {
            throw new ConflictException("Only pending team memberships can be declined.");
        }
        var hasActiveApplications = await _unitOfWork.Applications.Query().AnyAsync(a =>
            a.TeamId == member.TeamId &&
            (a.Status == ApplicationStatus.Submitted ||
             a.Status == ApplicationStatus.Accepted ||
             a.Status == ApplicationStatus.Attended),
            cancellationToken);
        if (hasActiveApplications)
        {
            throw new ConflictException("Team membership cannot be changed while the team has active applications.");
        }
        member.Status = TeamMemberStatus.Declined;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TeamMemberDto>(member);
    }
}
