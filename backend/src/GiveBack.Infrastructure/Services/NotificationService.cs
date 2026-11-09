using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly GiveBackDbContext _dbContext;

    public NotificationService(GiveBackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateAsync(int userId, string title, string message, NotificationType type, CancellationToken cancellationToken = default, string? relatedEntityType = null, int? relatedEntityId = null)
    {
        await _dbContext.Notifications.AddAsync(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    public async Task NotifyMatchingVolunteersAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        if (opportunity.Visibility != OpportunityVisibility.Public)
        {
            return;
        }

        var skillIds = await _dbContext.OpportunitySkills
            .Where(os => os.OpportunityId == opportunity.Id)
            .Select(os => os.SkillId)
            .ToListAsync(cancellationToken);
        var volunteers = await _dbContext.VolunteerProfiles
            .Include(v => v.VolunteerSkills)
            .Include(v => v.VolunteerInterests)
            .Where(v => v.User.IsActive &&
                        (v.VolunteerInterests.Any(i => i.CauseId == opportunity.CauseId) ||
                         v.VolunteerSkills.Any(s => skillIds.Contains(s.SkillId))))
            .Select(v => new { v.UserId })
            .ToListAsync(cancellationToken);
        foreach (var volunteer in volunteers)
        {
            await CreateAsync(volunteer.UserId, "New matching opportunity", opportunity.Title, NotificationType.NewMatchingOpportunity, cancellationToken);
        }
    }
}
