using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Notifications;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Opportunities.CancelOpportunity;

public sealed class CancelOpportunityCommandHandler : IRequestHandler<CancelOpportunityCommand, OpportunityDetailsDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelOpportunityCommandHandler> _logger;

    public CancelOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, ILogger<CancelOpportunityCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OpportunityDetailsDto> Handle(CancelOpportunityCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can cancel this opportunity.");
        }
        if (opportunity.Status is not (OpportunityStatus.Open or OpportunityStatus.Filled))
        {
            throw new ConflictException("Only open or filled opportunities can be cancelled.");
        }

        opportunity.Status = OpportunityStatus.Cancelled;
        foreach (var application in opportunity.Applications.Where(a => a.Status == ApplicationStatus.Accepted || a.Status == ApplicationStatus.Attended))
        {
            foreach (var userId in ApplicationNotificationRecipients.GetUserIds(application))
            {
                await _notificationService.CreateAsync(userId, "Opportunity cancelled", $"'{opportunity.Title}' has been cancelled.", NotificationType.OpportunityCancelled, cancellationToken);
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Opportunity {OpportunityId} status changed to {Status}", opportunity.Id, opportunity.Status);
        return _mapper.Map<OpportunityDetailsDto>(opportunity);
    }
}
