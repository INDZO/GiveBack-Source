using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveBack.Infrastructure.Services;

public sealed class BadgeService : IBadgeService
{
    private readonly GiveBackDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BadgeService> _logger;

    public BadgeService(GiveBackDbContext dbContext, INotificationService notificationService, ILogger<BadgeService> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task EvaluateAndAwardBadgesAsync(int volunteerId, CancellationToken cancellationToken = default)
    {
        var volunteer = await _dbContext.VolunteerProfiles.Include(v => v.User).FirstAsync(v => v.Id == volunteerId, cancellationToken);
        var logs = _dbContext.VolunteerHourLogs
            .Include(h => h.Opportunity)
            .Where(h => h.VolunteerId == volunteerId && (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected));

        if (await logs.AnyAsync(cancellationToken))
        {
            await AwardAsync(volunteer, "First hour", cancellationToken);
        }
        if (volunteer.TotalHours >= 100)
        {
            await AwardAsync(volunteer, "100 hours", cancellationToken);
        }
        if (await logs.Select(h => h.Opportunity.CauseId).Distinct().CountAsync(cancellationToken) >= 5)
        {
            await AwardAsync(volunteer, "5 different causes", cancellationToken);
        }
        var now = DateTime.UtcNow;
        if (await logs.Where(h => h.WorkDate.Year == now.Year && h.WorkDate.Month == now.Month)
            .Select(h => h.WorkDate.Date)
            .Distinct()
            .CountAsync(cancellationToken) >= 4)
        {
            await AwardAsync(volunteer, "Monthly regular", cancellationToken);
        }
        if (await logs.AnyAsync(h => h.Opportunity.IsUrgent, cancellationToken))
        {
            await AwardAsync(volunteer, "Crisis response", cancellationToken);
        }
    }

    private async Task AwardAsync(VolunteerProfile volunteer, string badgeName, CancellationToken cancellationToken)
    {
        var badge = await _dbContext.Badges.FirstOrDefaultAsync(b => b.Name == badgeName, cancellationToken);
        if (badge is null)
        {
            return;
        }
        var alreadyAwarded = await _dbContext.VolunteerBadges.AnyAsync(vb => vb.VolunteerId == volunteer.Id && vb.BadgeId == badge.Id, cancellationToken);
        if (alreadyAwarded)
        {
            return;
        }
        await _dbContext.VolunteerBadges.AddAsync(new VolunteerBadge
        {
            VolunteerId = volunteer.Id,
            BadgeId = badge.Id,
            AwardedAt = DateTime.UtcNow
        }, cancellationToken);
        await _notificationService.CreateAsync(volunteer.UserId, "Milestone badge earned", badge.Name, NotificationType.Milestone, cancellationToken);
        _logger.LogInformation("Awarded badge {BadgeName} to volunteer {VolunteerId}", badge.Name, volunteer.Id);
    }
}
