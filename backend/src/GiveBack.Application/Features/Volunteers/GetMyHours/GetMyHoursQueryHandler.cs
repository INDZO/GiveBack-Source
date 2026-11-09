using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Volunteers.GetMyHours;

public sealed class GetMyHoursQueryHandler : IRequestHandler<GetMyHoursQuery, IReadOnlyList<VolunteerHourLogDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetMyHoursQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VolunteerHourLogDto>> Handle(GetMyHoursQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        return await _unitOfWork.HourLogs.Query()
            .Where(h => h.VolunteerId == profile.Id)
            .OrderByDescending(h => h.WorkDate)
            .ThenByDescending(h => h.SubmittedAt)
            .ProjectTo<VolunteerHourLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
