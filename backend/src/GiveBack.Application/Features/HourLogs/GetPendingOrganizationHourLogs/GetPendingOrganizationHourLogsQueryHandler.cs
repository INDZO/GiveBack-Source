using AutoMapper;
using AutoMapper.QueryableExtensions;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.HourLogs.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.HourLogs.GetPendingOrganizationHourLogs;

public sealed class GetPendingOrganizationHourLogsQueryHandler : IRequestHandler<GetPendingOrganizationHourLogsQuery, IReadOnlyList<VolunteerHourLogDto>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPendingOrganizationHourLogsQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<VolunteerHourLogDto>> Handle(GetPendingOrganizationHourLogsQuery request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        return await _unitOfWork.HourLogs.Query()
            .Where(h => h.Opportunity.OrganizationId == organization.Id && h.Status == HourLogStatus.Pending)
            .OrderBy(h => h.WorkDate)
            .ThenBy(h => h.SubmittedAt)
            .ProjectTo<VolunteerHourLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
