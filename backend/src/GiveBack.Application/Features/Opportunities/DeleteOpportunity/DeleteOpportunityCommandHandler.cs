using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.DeleteOpportunity;

public sealed class DeleteOpportunityCommandHandler : IRequestHandler<DeleteOpportunityCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOpportunityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteOpportunityCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var opportunity = await _unitOfWork.Opportunities.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Opportunity was not found.");
        if (opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can delete this opportunity.");
        }
        if (opportunity.Applications.Count != 0 || opportunity.HourLogs.Count != 0 || opportunity.ImpactReport is not null || opportunity.Invitations.Count != 0)
        {
            throw new ConflictException("Opportunity has related data and cannot be deleted. Cancel it instead.");
        }

        _unitOfWork.Opportunities.Remove(opportunity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
