using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.UpdateAvailability;

public sealed class UpdateAvailabilityCommandHandler : IRequestHandler<UpdateAvailabilityCommand, IReadOnlyList<VolunteerAvailabilityDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAvailabilityCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VolunteerAvailabilityDto>> Handle(UpdateAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Volunteers.GetByUserIdWithDetailsAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        foreach (var existing in profile.Availabilities.ToList())
        {
            _unitOfWork.Repository<VolunteerAvailability>().Remove(existing);
        }
        foreach (var item in request.Availability)
        {
            if (item.StartTime >= item.EndTime)
            {
                throw new ConflictException("Availability start time must be before end time.");
            }
            profile.Availabilities.Add(new VolunteerAvailability
            {
                VolunteerId = profile.Id,
                DayOfWeek = item.DayOfWeek,
                StartTime = item.StartTime,
                EndTime = item.EndTime
            });
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<VolunteerAvailabilityDto>>(profile.Availabilities);
    }
}
