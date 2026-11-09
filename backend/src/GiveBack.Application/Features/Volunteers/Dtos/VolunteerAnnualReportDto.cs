namespace GiveBack.Application.Features.Volunteers.Dtos;

public sealed class VolunteerAnnualReportDto
{
    public int Year { get; set; }
    public IReadOnlyList<MonthlyHoursDto> HoursByMonth { get; set; } = Array.Empty<MonthlyHoursDto>();
    public int OpportunitiesCount { get; set; }
    public int CausesCount { get; set; }
    public int EarnedBadgesCount { get; set; }
}
