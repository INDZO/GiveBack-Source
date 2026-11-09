using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;

namespace GiveBack.Application.Common.Interfaces;

public interface INotificationService
{
    Task CreateAsync(int userId, string title, string message, NotificationType type, CancellationToken cancellationToken = default, string? relatedEntityType = null, int? relatedEntityId = null);
    Task NotifyMatchingVolunteersAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
}
