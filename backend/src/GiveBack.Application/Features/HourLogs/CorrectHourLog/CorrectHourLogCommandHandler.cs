using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.HourLogs.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.HourLogs.CorrectHourLog;

public sealed class CorrectHourLogCommandHandler : IRequestHandler<CorrectHourLogCommand, VolunteerHourLogDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBadgeService _badgeService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;
    private readonly ILogger<CorrectHourLogCommandHandler> _logger;

    public CorrectHourLogCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IBadgeService badgeService, INotificationService notificationService, IMapper mapper, ILogger<CorrectHourLogCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _badgeService = badgeService;
        _notificationService = notificationService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<VolunteerHourLogDto> Handle(CorrectHourLogCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
                ?? throw new NotFoundException("Organization profile was not found.");
            var hourLog = await _unitOfWork.HourLogs.GetDetailsAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException("Hour log was not found.");
            if (hourLog.Opportunity.OrganizationId != organization.Id)
            {
                throw new ForbiddenException("Only the owning organization can verify this hour log.");
            }
            if (hourLog.Status != HourLogStatus.Pending)
            {
                throw new ConflictException("Only pending hour logs can be verified.");
            }
            hourLog.ApprovedHours = request.ApprovedHours;
            hourLog.Status = HourLogStatus.Corrected;
            hourLog.VerifiedAt = DateTime.UtcNow;
            hourLog.VerifiedByOrganizationId = organization.Id;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var volunteer = await _unitOfWork.Volunteers.GetDetailsAsync(hourLog.VolunteerId, cancellationToken)
                ?? throw new NotFoundException("Volunteer profile was not found.");
            volunteer.TotalHours = await _unitOfWork.HourLogs.Query()
                .Where(h => h.VolunteerId == volunteer.Id && (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected))
                .SumAsync(h => h.ApprovedHours ?? 0, cancellationToken);

            await _badgeService.EvaluateAndAwardBadgesAsync(volunteer.Id, cancellationToken);
            await _notificationService.CreateAsync(volunteer.UserId, "Hours verified", hourLog.Opportunity.Title, NotificationType.HoursVerified, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Hour log {HourLogId} changed to {Status}", hourLog.Id, hourLog.Status);
            return _mapper.Map<VolunteerHourLogDto>(hourLog);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
