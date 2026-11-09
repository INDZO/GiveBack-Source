using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Policies;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.InviteVolunteer;

public sealed class InviteVolunteerCommandHandler : IRequestHandler<InviteVolunteerCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public InviteVolunteerCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task Handle(InviteVolunteerCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.OpportunityId, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can invite volunteers.");
        }
        if (opportunity.Visibility != OpportunityVisibility.Private)
        {
            throw new ConflictException("Invitations are only needed for private opportunities.");
        }
        if (opportunity.Status != OpportunityStatus.Open)
        {
            throw new ConflictException("Only open opportunities can receive invitations.");
        }
        if (OpportunityTimeRules.HasEnded(opportunity))
        {
            throw new ConflictException("Invitations are closed because the opportunity has already ended.");
        }
        var volunteer = await _unitOfWork.Volunteers.GetDetailsAsync(request.VolunteerId, cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var duplicate = await _unitOfWork.Repository<OpportunityInvitation>().Query()
            .AnyAsync(i => i.OpportunityId == opportunity.Id && i.VolunteerId == volunteer.Id, cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("An invitation already exists for this volunteer and opportunity.");
        }
        await _unitOfWork.Repository<OpportunityInvitation>().AddAsync(new OpportunityInvitation
        {
            OpportunityId = opportunity.Id,
            VolunteerId = volunteer.Id,
            Status = InvitationStatus.Pending
        }, cancellationToken);
        await _notificationService.CreateAsync(volunteer.UserId, "Private opportunity invitation", $"You were invited to '{opportunity.Title}'.", NotificationType.InvitationReceived, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
