using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Applications.GetMyApplications;

public sealed class GetMyApplicationsQueryHandler : IRequestHandler<GetMyApplicationsQuery, IReadOnlyList<ApplicationDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyApplicationsQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ApplicationDto>> Handle(GetMyApplicationsQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        return await _unitOfWork.Applications.Query()
            .Where(a =>
                a.VolunteerId == volunteer.Id ||
                (a.Team != null &&
                 (a.Team.CaptainVolunteerId == volunteer.Id ||
                  a.Team.Members.Any(m => m.VolunteerId == volunteer.Id && m.Status == TeamMemberStatus.Confirmed))))
            .OrderByDescending(a => a.SubmittedAt)
            .ProjectTo<ApplicationDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
