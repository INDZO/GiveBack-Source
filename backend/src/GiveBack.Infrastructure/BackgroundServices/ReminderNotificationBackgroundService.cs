using GiveBack.Application.Common.Notifications;
using GiveBack.Domain.Enums;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GiveBack.Infrastructure.BackgroundServices;

public sealed class ReminderNotificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderNotificationBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public ReminderNotificationBackgroundService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<ReminderNotificationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(configuration.GetValue<int?>("ReminderNotifications:IntervalSeconds") ?? 300);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reminder notification execution failed.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveBackDbContext>();
        var now = DateTime.UtcNow;
        var until = now.AddHours(24);
        var applications = await dbContext.Applications
            .Include(a => a.Opportunity)
            .Include(a => a.Volunteer).ThenInclude(v => v!.User)
            .Include(a => a.Team).ThenInclude(t => t!.CaptainVolunteer).ThenInclude(v => v.User)
            .Include(a => a.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.Volunteer).ThenInclude(v => v.User)
            .Where(a => a.Status == ApplicationStatus.Accepted &&
                        a.Opportunity.StartAt >= now &&
                        a.Opportunity.StartAt <= until)
            .ToListAsync(cancellationToken);

        var created = 0;
        foreach (var application in applications)
        {
            foreach (var userId in ApplicationNotificationRecipients.GetUserIds(application))
            {
                var message = $"Opportunity {application.Opportunity.Id} starts within 24 hours.";
                var duplicate = await dbContext.Notifications.AnyAsync(n =>
                    n.UserId == userId &&
                    n.NotificationType == NotificationType.Reminder24h &&
                    n.RelatedEntityType == "Opportunity" &&
                    n.RelatedEntityId == application.OpportunityId,
                    cancellationToken);
                if (duplicate)
                {
                    continue;
                }

                await dbContext.Notifications.AddAsync(new Domain.Entities.Notification
                {
                    UserId = userId,
                    Title = "Upcoming opportunity reminder",
                    Message = message,
                    NotificationType = NotificationType.Reminder24h,
                    RelatedEntityType = "Opportunity",
                    RelatedEntityId = application.OpportunityId,
                    CreatedAt = now
                }, cancellationToken);
                created++;
            }
        }

        if (created > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        _logger.LogInformation("Reminder notification execution completed. Created {Count} reminders.", created);
    }
}
