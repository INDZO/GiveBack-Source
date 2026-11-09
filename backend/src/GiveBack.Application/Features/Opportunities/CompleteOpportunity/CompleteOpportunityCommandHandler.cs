using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Notifications;
using GiveBack.Application.Common.Policies;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Opportunities.CompleteOpportunity;

public sealed class CompleteOpportunityCommandHandler : IRequestHandler<CompleteOpportunityCommand, OpportunityDetailsDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CompleteOpportunityCommandHandler> _logger;

    public CompleteOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, ILogger<CompleteOpportunityCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OpportunityDetailsDto> Handle(CompleteOpportunityCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can complete this opportunity.");
        }
        if (opportunity.Status is not (OpportunityStatus.Open or OpportunityStatus.Filled))
        {
            throw new ConflictException("Only open or filled opportunities can be completed.");
        }
        if (!OpportunityTimeRules.HasEnded(opportunity))
        {
            throw new ConflictException("Opportunity cannot be completed before it ends.");
        }
        var successfulApplications = opportunity.Applications
            .Where(a => a.Status == ApplicationStatus.Accepted || a.Status == ApplicationStatus.Attended)
            .ToList();
        if (successfulApplications.Count == 0)
        {
            throw new ConflictException("Opportunity cannot be completed without accepted or attended applications.");
        }

        opportunity.Status = OpportunityStatus.Completed;
        foreach (var application in successfulApplications)
        {
            foreach (var userId in ApplicationNotificationRecipients.GetUserIds(application))
            {
                await _notificationService.CreateAsync(userId, "Opportunity completed", $"'{opportunity.Title}' has been completed.", NotificationType.OpportunityCompleted, cancellationToken);
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Opportunity {OpportunityId} status changed to {Status}", opportunity.Id, opportunity.Status);
        return _mapper.Map<OpportunityDetailsDto>(opportunity);
    }
}
