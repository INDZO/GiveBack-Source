using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Opportunities.GetMyInvitations;

public sealed class GetMyInvitationsQueryHandler : IRequestHandler<GetMyInvitationsQuery, IReadOnlyList<OpportunityInvitationDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyInvitationsQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<OpportunityInvitationDto>> Handle(GetMyInvitationsQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");

        return await _unitOfWork.Repository<GiveBack.Domain.Entities.OpportunityInvitation>().Query()
            .Where(i => i.VolunteerId == volunteer.Id)
            .OrderByDescending(i => i.InvitedAt)
            .ProjectTo<OpportunityInvitationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
