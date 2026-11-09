using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationReadCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.RequireUserId();
        var notification = await _unitOfWork.Repository<Notification>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Notification was not found.");
        if (notification.UserId != userId)
        {
            throw new ForbiddenException("Notification does not belong to the current user.");
        }
        notification.IsRead = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
