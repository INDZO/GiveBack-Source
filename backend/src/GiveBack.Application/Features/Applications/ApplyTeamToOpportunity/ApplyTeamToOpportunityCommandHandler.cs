using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Policies;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Application.Features.Applications.ApplyTeamToOpportunity;

public sealed class ApplyTeamToOpportunityCommandHandler : IRequestHandler<ApplyTeamToOpportunityCommand, ApplicationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public ApplyTeamToOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<ApplicationDto> Handle(ApplyTeamToOpportunityCommand request, CancellationToken cancellationToken)
    {
        var captain = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var team = await _unitOfWork.Repository<VolunteerTeam>().Query()
            .Include(t => t.Members)
            .ThenInclude(m => m.Volunteer)
            .FirstOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken)
            ?? throw new NotFoundException("Team was not found.");
        if (team.CaptainVolunteerId != captain.Id)
        {
            throw new ForbiddenException("Only the captain can apply with the team.");
        }

        var opportunity = await _unitOfWork.Opportunities.GetForApplicationAsync(request.OpportunityId, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (!opportunity.IsTeamBased)
        {
            throw new ConflictException("Team application is allowed only for team-based opportunities.");
        }
        if (opportunity.Status != OpportunityStatus.Open)
        {
            throw new ConflictException("Only open opportunities can receive applications.");
        }
        if (OpportunityTimeRules.HasEnded(opportunity))
        {
            throw new ConflictException("Applications are closed because the opportunity has already ended.");
        }

        if (team.Members.Any(m => m.Status == TeamMemberStatus.Pending))
        {
            throw new ConflictException("All invited team members must confirm or decline before the team can apply.");
        }

        var confirmedMemberIds = team.Members
            .Where(m => m.Status == TeamMemberStatus.Confirmed)
            .Select(m => m.VolunteerId)
            .Distinct()
            .ToArray();
        var confirmedCount = confirmedMemberIds.Length;

        var confirmedNonCaptainCount = confirmedMemberIds.Count(id => id != team.CaptainVolunteerId);
        if (confirmedNonCaptainCount < 1)
        {
            throw new ConflictException("A team needs at least one confirmed member besides the captain before it can apply.");
        }

        if (opportunity.Visibility == OpportunityVisibility.Private)
        {
            var invitedVolunteerIds = opportunity.Invitations
                .Where(i => i.Status == InvitationStatus.Accepted)
                .Select(i => i.VolunteerId)
                .ToHashSet();
            if (confirmedMemberIds.Any(id => !invitedVolunteerIds.Contains(id)))
            {
                throw new ForbiddenException("Private team opportunity requires accepted invitations for all confirmed team members.");
            }
        }

        if (opportunity.AcceptedVolunteersCount + confirmedCount > opportunity.RequiredVolunteersCount)
        {
            throw new ConflictException("Opportunity capacity cannot be exceeded.");
        }

        var duplicateStatuses = new[] { ApplicationStatus.Submitted, ApplicationStatus.Accepted, ApplicationStatus.Attended, ApplicationStatus.NoShow };
        var duplicate = await _unitOfWork.Applications.Query().AnyAsync(a =>
            a.OpportunityId == opportunity.Id && duplicateStatuses.Contains(a.Status) && a.TeamId == team.Id,
            cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("Same team cannot apply twice to the same opportunity.");
        }
        var duplicateParticipant = await _unitOfWork.Applications.Query().AnyAsync(a =>
            a.OpportunityId == opportunity.Id &&
            duplicateStatuses.Contains(a.Status) &&
            ((a.VolunteerId.HasValue && confirmedMemberIds.Contains(a.VolunteerId.Value)) ||
             (a.Team != null && a.Team.Members.Any(m => confirmedMemberIds.Contains(m.VolunteerId) && m.Status == TeamMemberStatus.Confirmed))),
            cancellationToken);
        if (duplicateParticipant)
        {
            throw new ConflictException("One or more confirmed team members already have an application for this opportunity.");
        }

        var openStatuses = new[] { ApplicationStatus.Submitted, ApplicationStatus.Accepted };
        var limit = int.TryParse(_configuration["ApplicationRules:MaxOpenApplicationsPerVolunteer"], out var configuredLimit) ? configuredLimit : 5;
        foreach (var memberId in confirmedMemberIds)
        {
            var openCount = await _unitOfWork.Applications.Query().CountAsync(a =>
                openStatuses.Contains(a.Status) &&
                (a.Opportunity.Status == OpportunityStatus.Open || a.Opportunity.Status == OpportunityStatus.Filled) &&
                (a.VolunteerId == memberId ||
                 (a.Team != null && a.Team.Members.Any(m => m.VolunteerId == memberId && m.Status == TeamMemberStatus.Confirmed))),
                cancellationToken);
            if (openCount >= limit)
            {
                throw new ConflictException("Maximum number of open applications reached for one or more confirmed team members.");
            }
        }

        var application = new VolunteerApplication
        {
            OpportunityId = opportunity.Id,
            TeamId = team.Id,
            ApplicationType = ApplicationType.Team,
            Status = ApplicationStatus.Submitted,
            Message = request.Message
        };
        await _unitOfWork.Applications.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _unitOfWork.Applications.GetDetailsAsync(application.Id, cancellationToken) ?? application;
        return _mapper.Map<ApplicationDto>(saved);
    }
}
