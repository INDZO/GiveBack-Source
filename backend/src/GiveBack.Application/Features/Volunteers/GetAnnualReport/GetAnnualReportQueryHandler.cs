using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Volunteers.GetAnnualReport;

public sealed class GetAnnualReportQueryHandler : IRequestHandler<GetAnnualReportQuery, VolunteerAnnualReportDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public GetAnnualReportQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<VolunteerAnnualReportDto> Handle(GetAnnualReportQuery request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var logs = await _unitOfWork.HourLogs.Query()
            .Where(h => h.VolunteerId == profile.Id &&
                        (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected) &&
                        h.WorkDate.Year == request.Year)
            .Select(h => new { h.WorkDate, Hours = h.ApprovedHours ?? 0, h.OpportunityId, h.Opportunity.CauseId })
            .ToListAsync(cancellationToken);
        var badges = await _unitOfWork.Repository<VolunteerBadge>().Query()
            .CountAsync(vb => vb.VolunteerId == profile.Id && vb.AwardedAt.Year == request.Year, cancellationToken);

        return new VolunteerAnnualReportDto
        {
            Year = request.Year,
            HoursByMonth = Enumerable.Range(1, 12)
                .Select(month => new MonthlyHoursDto
                {
                    Month = month,
                    TotalHours = logs.Where(l => l.WorkDate.Month == month).Sum(l => l.Hours)
                })
                .ToList(),
            OpportunitiesCount = logs.Select(l => l.OpportunityId).Distinct().Count(),
            CausesCount = logs.Select(l => l.CauseId).Distinct().Count(),
            EarnedBadgesCount = badges
        };
    }
}
