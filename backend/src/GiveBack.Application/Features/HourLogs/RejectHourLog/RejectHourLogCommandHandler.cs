using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.HourLogs.Dtos;
using GiveBack.Domain.Enums;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.RejectHourLog;

public sealed class RejectHourLogCommandHandler : IRequestHandler<RejectHourLogCommand, VolunteerHourLogDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RejectHourLogCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<VolunteerHourLogDto> Handle(RejectHourLogCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var hourLog = await _unitOfWork.HourLogs.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Hour log was not found.");
        if (hourLog.Opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can reject this hour log.");
        }
        if (hourLog.Status != HourLogStatus.Pending)
        {
            throw new ConflictException("Only pending hour logs can be rejected.");
        }
        hourLog.Status = HourLogStatus.Rejected;
        hourLog.VerifiedAt = DateTime.UtcNow;
        hourLog.VerifiedByOrganizationId = organization.Id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<VolunteerHourLogDto>(hourLog);
    }
}
