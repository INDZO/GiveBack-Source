using GiveBack.Application.Features.Notifications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Notifications.GetNotifications;

public sealed record GetNotificationsQuery : IRequest<IReadOnlyList<NotificationDto>>;
