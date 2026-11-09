using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Notifications;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Admin.CancelOpportunityAsAdmin;

public sealed class CancelOpportunityAsAdminCommandHandler : IRequestHandler<CancelOpportunityAsAdminCommand, OpportunityDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelOpportunityAsAdminCommandHandler> _logger;

    public CancelOpportunityAsAdminCommandHandler(IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, ILogger<CancelOpportunityAsAdminCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OpportunityDto> Handle(CancelOpportunityAsAdminCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.Status is not (OpportunityStatus.Open or OpportunityStatus.Filled))
        {
            throw new ConflictException("Only open or filled opportunities can be cancelled.");
        }

        opportunity.Status = OpportunityStatus.Cancelled;
        foreach (var application in opportunity.Applications.Where(a => a.Status == ApplicationStatus.Accepted || a.Status == ApplicationStatus.Attended))
        {
            foreach (var userId in ApplicationNotificationRecipients.GetUserIds(application))
            {
                await _notificationService.CreateAsync(userId, "Opportunity cancelled", $"'{opportunity.Title}' has been cancelled by an administrator.", NotificationType.OpportunityCancelled, cancellationToken);
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin cancelled opportunity {OpportunityId}", opportunity.Id);
        return _mapper.Map<OpportunityDto>(opportunity);
    }
}
