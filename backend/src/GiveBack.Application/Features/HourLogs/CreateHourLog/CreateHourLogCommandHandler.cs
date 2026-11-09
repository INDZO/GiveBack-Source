using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Policies;
using GiveBack.Application.Features.HourLogs.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.HourLogs.CreateHourLog;

public sealed class CreateHourLogCommandHandler : IRequestHandler<CreateHourLogCommand, VolunteerHourLogDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateHourLogCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VolunteerHourLogDto> Handle(CreateHourLogCommand request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var application = await _unitOfWork.Applications.GetDetailsAsync(request.ApplicationId, cancellationToken)
            ?? throw new NotFoundException("Application was not found.");
        if (application.Status != ApplicationStatus.Accepted && application.Status != ApplicationStatus.Attended)
        {
            throw new ConflictException("Hours can be submitted only for accepted or attended applications.");
        }
        var belongs = application.VolunteerId == volunteer.Id ||
            (application.Team?.Members.Any(m => m.VolunteerId == volunteer.Id && m.Status == TeamMemberStatus.Confirmed) ?? false);
        if (!belongs)
        {
            throw new ForbiddenException("Application does not belong to the current volunteer.");
        }
        var workDate = request.WorkDate.Date;
        if (workDate > DateTime.UtcNow.Date)
        {
            throw new ConflictException("Work date cannot be in the future.");
        }
        if (workDate < application.Opportunity.StartAt.Date)
        {
            throw new ConflictException("Work date cannot be before the opportunity start date.");
        }
        if (workDate > OpportunityTimeRules.EndsAt(application.Opportunity).Date)
        {
            throw new ConflictException("Work date cannot be after the opportunity end date.");
        }
        var duplicate = await _unitOfWork.HourLogs.Query().AnyAsync(h =>
            h.ApplicationId == application.Id &&
            h.VolunteerId == volunteer.Id &&
            h.WorkDate == workDate,
            cancellationToken);
        if (duplicate)
        {
            throw new ConflictException("Hour log already exists for this application and work date.");
        }
        var log = new VolunteerHourLog
        {
            VolunteerId = volunteer.Id,
            OpportunityId = application.OpportunityId,
            ApplicationId = application.Id,
            SubmittedHours = request.SubmittedHours,
            Description = request.Description.Trim(),
            WorkDate = workDate,
            Status = HourLogStatus.Pending
        };
        await _unitOfWork.HourLogs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _unitOfWork.HourLogs.GetDetailsAsync(log.Id, cancellationToken) ?? log;
        return _mapper.Map<VolunteerHourLogDto>(saved);
    }
}
