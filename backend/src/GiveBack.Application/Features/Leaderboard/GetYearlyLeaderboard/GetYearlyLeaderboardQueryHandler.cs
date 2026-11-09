using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Leaderboard.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Leaderboard.GetYearlyLeaderboard;

public sealed class GetYearlyLeaderboardQueryHandler : IRequestHandler<GetYearlyLeaderboardQuery, IReadOnlyList<LeaderboardEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetYearlyLeaderboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LeaderboardEntryDto>> Handle(GetYearlyLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var entries = await _unitOfWork.HourLogs.Query()
            .Where(h => (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected) &&
                        h.WorkDate.Year == request.Year)
            .GroupBy(h => new { h.VolunteerId, h.Volunteer.FirstName, h.Volunteer.LastName })
            .Select(g => new LeaderboardEntryDto
            {
                VolunteerId = g.Key.VolunteerId,
                FirstName = g.Key.FirstName,
                LastName = g.Key.LastName,
                TotalHours = g.Sum(h => h.ApprovedHours ?? 0),
                NumberOfOpportunities = g.Select(h => h.OpportunityId).Distinct().Count()
            })
            .OrderByDescending(e => e.TotalHours)
            .ThenBy(e => e.LastName)
            .ToListAsync(cancellationToken);
        for (var i = 0; i < entries.Count; i++)
        {
            entries[i].Rank = i + 1;
        }
        return entries;
    }
}
