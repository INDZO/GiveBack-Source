using MediatR;

namespace GiveBack.Application.Features.Notifications.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(int Id) : IRequest;
