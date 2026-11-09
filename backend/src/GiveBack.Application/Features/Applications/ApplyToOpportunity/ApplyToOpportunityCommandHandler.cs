using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Policies;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Application.Features.Applications.ApplyToOpportunity;

public sealed class ApplyToOpportunityCommandHandler : IRequestHandler<ApplyToOpportunityCommand, ApplicationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public ApplyToOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<ApplicationDto> Handle(ApplyToOpportunityCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdWithDetailsAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetForApplicationAsync(request.OpportunityId, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.Status != OpportunityStatus.Open)
        {
            throw new ConflictException("Only open opportunities can receive applications.");
        }
        if (OpportunityTimeRules.HasEnded(opportunity))
        {
            throw new ConflictException("Applications are closed because the opportunity has already ended.");
        }
        if (opportunity.IsTeamBased)
        {
            throw new ConflictException("Individual applications are not allowed for team-based opportunities.");
        }
        if (opportunity.Visibility == OpportunityVisibility.Private &&
            !opportunity.Invitations.Any(i => i.VolunteerId == volunteer.Id && i.Status == InvitationStatus.Accepted))
        {
            throw new ForbiddenException("Private opportunity requires an accepted invitation.");
        }
        if (opportunity.AcceptedVolunteersCount + 1 > opportunity.RequiredVolunteersCount)
        {
            throw new ConflictException("Opportunity capacity cannot be exceeded.");
        }

        var duplicateStatuses = new[] { ApplicationStatus.Submitted, ApplicationStatus.Accepted, ApplicationStatus.Attended, ApplicationStatus.NoShow };
        var duplicate = await _unitOfWork.Applications.Query().AnyAsync(a =>
            a.OpportunityId == opportunity.Id &&
            duplicateStatuses.Contains(a.Status) &&
            (a.VolunteerId == volunteer.Id ||
             (a.Team != null && a.Team.Members.Any(m => m.VolunteerId == volunteer.Id && m.Status == TeamMemberStatus.Confirmed))),
            cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("Duplicate application is not allowed.");
        }

        var openStatuses = new[] { ApplicationStatus.Submitted, ApplicationStatus.Accepted };
        var limit = int.TryParse(_configuration["ApplicationRules:MaxOpenApplicationsPerVolunteer"], out var configuredLimit) ? configuredLimit : 5;
        var openCount = await _unitOfWork.Applications.Query().CountAsync(a =>
            openStatuses.Contains(a.Status) &&
            (a.Opportunity.Status == OpportunityStatus.Open || a.Opportunity.Status == OpportunityStatus.Filled) &&
            (a.VolunteerId == volunteer.Id ||
             (a.Team != null && a.Team.Members.Any(m => m.VolunteerId == volunteer.Id && m.Status == TeamMemberStatus.Confirmed))),
            cancellationToken);
        if (openCount >= limit)
        {
            throw new ConflictException("Maximum number of open applications reached.");
        }

        var application = new VolunteerApplication
        {
            OpportunityId = opportunity.Id,
            VolunteerId = volunteer.Id,
            ApplicationType = ApplicationType.Individual,
            Status = ApplicationStatus.Submitted,
            Message = request.Message
        };
        await _unitOfWork.Applications.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _unitOfWork.Applications.GetDetailsAsync(application.Id, cancellationToken) ?? application;
        return _mapper.Map<ApplicationDto>(saved);
    }
}
