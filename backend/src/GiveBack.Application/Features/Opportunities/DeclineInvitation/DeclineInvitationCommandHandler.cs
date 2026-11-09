using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.DeclineInvitation;

public sealed class DeclineInvitationCommandHandler : IRequestHandler<DeclineInvitationCommand, OpportunityInvitationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public DeclineInvitationCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, INotificationService notificationService, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<OpportunityInvitationDto> Handle(DeclineInvitationCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var invitation = await _unitOfWork.Repository<GiveBack.Domain.Entities.OpportunityInvitation>().Query()
            .Include(i => i.Opportunity)
            .ThenInclude(o => o.Organization)
            .ThenInclude(o => o.User)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Invitation was not found.");
        if (invitation.VolunteerId != volunteer.Id)
        {
            throw new ForbiddenException("Only the invited volunteer can decline this invitation.");
        }
        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new ConflictException("Only pending invitations can be declined.");
        }

        invitation.Status = InvitationStatus.Declined;
        await _notificationService.CreateAsync(invitation.Opportunity.Organization.UserId, "Invitation declined", $"{volunteer.FirstName} {volunteer.LastName} declined the invitation for '{invitation.Opportunity.Title}'.", NotificationType.InvitationDeclined, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OpportunityInvitationDto>(invitation);
    }
}
