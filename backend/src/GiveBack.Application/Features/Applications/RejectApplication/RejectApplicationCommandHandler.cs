using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Notifications;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Applications.RejectApplication;

public sealed class RejectApplicationCommandHandler : IRequestHandler<RejectApplicationCommand, ApplicationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<RejectApplicationCommandHandler> _logger;

    public RejectApplicationCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, ILogger<RejectApplicationCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApplicationDto> Handle(RejectApplicationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var application = await _unitOfWork.Applications.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Application was not found.");
        if (application.Opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can reject this application.");
        }
        if (application.Status != ApplicationStatus.Submitted)
        {
            throw new ConflictException("Invalid application status transition.");
        }
        application.Status = ApplicationStatus.Rejected;
        application.ReviewedAt = DateTime.UtcNow;
        foreach (var userId in ApplicationNotificationRecipients.GetUserIds(application))
        {
            await _notificationService.CreateAsync(userId, "Application rejected", $"Your application for '{application.Opportunity.Title}' was rejected.", NotificationType.ApplicationRejected, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Application {ApplicationId} changed to {Status}", application.Id, application.Status);
        return _mapper.Map<ApplicationDto>(application);
    }
}
