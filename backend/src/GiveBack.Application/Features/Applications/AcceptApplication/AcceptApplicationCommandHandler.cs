using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Notifications;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Applications.AcceptApplication;

public sealed class AcceptApplicationCommandHandler : IRequestHandler<AcceptApplicationCommand, ApplicationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<AcceptApplicationCommandHandler> _logger;

    public AcceptApplicationCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper, ILogger<AcceptApplicationCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApplicationDto> Handle(AcceptApplicationCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
                ?? throw new NotFoundException("Organization profile was not found.");
            var application = await _unitOfWork.Applications.GetDetailsAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Application was not found.");
            var opportunity = application.Opportunity;

            if (opportunity.OrganizationId != organization.Id)
            {
                throw new ForbiddenException("Only the owning organization can accept this application.");
            }
            if (application.Status != ApplicationStatus.Submitted)
            {
                throw new ConflictException("Only submitted applications can be accepted.");
            }
            if (opportunity.Status != OpportunityStatus.Open)
            {
                throw new ConflictException("Only open opportunities can accept applications.");
            }
            if (opportunity.AcceptedVolunteersCount >= opportunity.RequiredVolunteersCount)
            {
                throw new ConflictException("Opportunity capacity is already full.");
            }

            var participantIds = application.ApplicationType == ApplicationType.Team
                ? application.Team?.Members
                    .Where(m => m.Status == TeamMemberStatus.Confirmed)
                    .Select(m => m.VolunteerId)
                    .Distinct()
                    .ToArray() ?? Array.Empty<int>()
                : application.VolunteerId.HasValue ? new[] { application.VolunteerId.Value } : Array.Empty<int>();
            var capacityAmount = participantIds.Length;
            if (capacityAmount <= 0)
            {
                throw new ConflictException("Application capacity amount must be positive.");
            }
            if (opportunity.AcceptedVolunteersCount + capacityAmount > opportunity.RequiredVolunteersCount)
            {
                throw new ConflictException("Opportunity capacity cannot be exceeded.");
            }
            var duplicateStatuses = new[] { ApplicationStatus.Submitted, ApplicationStatus.Accepted, ApplicationStatus.Attended, ApplicationStatus.NoShow };
            var duplicateParticipant = await _unitOfWork.Applications.Query().AnyAsync(a =>
                a.Id != application.Id &&
                a.OpportunityId == opportunity.Id &&
                duplicateStatuses.Contains(a.Status) &&
                ((a.VolunteerId.HasValue && participantIds.Contains(a.VolunteerId.Value)) ||
                 (a.Team != null && a.Team.Members.Any(m => participantIds.Contains(m.VolunteerId) && m.Status == TeamMemberStatus.Confirmed))),
                cancellationToken);
            if (duplicateParticipant)
            {
                throw new ConflictException("One or more application participants already have an active application for this opportunity.");
            }

            application.Status = ApplicationStatus.Accepted;
            application.ReviewedAt = DateTime.UtcNow;
            opportunity.AcceptedVolunteersCount += capacityAmount;
            if (opportunity.AcceptedVolunteersCount == opportunity.RequiredVolunteersCount)
            {
                opportunity.Status = OpportunityStatus.Filled;
            }

            foreach (var userId in ApplicationNotificationRecipients.GetUserIds(application))
            {
                await _notificationService.CreateAsync(userId, "Application accepted", $"Your application for '{opportunity.Title}' was accepted.", NotificationType.ApplicationAccepted, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Application {ApplicationId} accepted for opportunity {OpportunityId}", application.Id, opportunity.Id);
            return _mapper.Map<ApplicationDto>(application);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new ConflictException($"Application acceptance conflict: {ex.Message}");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
