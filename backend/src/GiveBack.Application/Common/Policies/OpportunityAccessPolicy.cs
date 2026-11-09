using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;

namespace GiveBack.Application.Common.Policies;

public sealed class OpportunityAccessPolicy : IOpportunityAccessPolicy
{
    private static readonly InvitationStatus[] VisibleInvitationStatuses =
    [
        InvitationStatus.Pending,
        InvitationStatus.Accepted
    ];

    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public OpportunityAccessPolicy(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CanViewAsync(Opportunity opportunity, CancellationToken cancellationToken = default)
    {
        if (opportunity.Visibility == OpportunityVisibility.Public)
        {
            return true;
        }

        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return false;
        }

        if (string.Equals(_currentUser.Role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (opportunity.Organization.UserId == userId.Value)
        {
            return true;
        }

        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(userId.Value, cancellationToken);
        if (volunteer is null)
        {
            return false;
        }

        if (opportunity.Invitations.Any(i =>
            i.VolunteerId == volunteer.Id &&
            VisibleInvitationStatuses.Contains(i.Status)))
        {
            return true;
        }

        return opportunity.Applications.Any(a =>
            a.VolunteerId == volunteer.Id ||
            (a.Team is not null &&
             (a.Team.CaptainVolunteerId == volunteer.Id ||
              a.Team.Members.Any(m => m.VolunteerId == volunteer.Id && m.Status == TeamMemberStatus.Confirmed))));
    }
}
