using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.HourLogs.Dtos;

public sealed class VolunteerHourLogDto
{
    public int Id { get; set; }
    public int VolunteerId { get; set; }
    public string VolunteerName { get; set; } = string.Empty;
    public int OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public int ApplicationId { get; set; }
    public decimal SubmittedHours { get; set; }
    public decimal? ApprovedHours { get; set; }
    public string Description { get; set; } = string.Empty;
    public HourLogStatus Status { get; set; }
    public DateTime WorkDate { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByOrganizationId { get; set; }
}
