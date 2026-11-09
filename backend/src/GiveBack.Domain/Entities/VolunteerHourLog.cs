using GiveBack.Domain.Enums;

namespace GiveBack.Domain.Entities;

public class VolunteerHourLog
{
    public int Id { get; set; }
    public int VolunteerId { get; set; }
    public int OpportunityId { get; set; }
    public int ApplicationId { get; set; }
    public decimal SubmittedHours { get; set; }
    public decimal? ApprovedHours { get; set; }
    public string Description { get; set; } = string.Empty;
    public HourLogStatus Status { get; set; } = HourLogStatus.Pending;
    public DateTime WorkDate { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public int? VerifiedByOrganizationId { get; set; }

    public VolunteerProfile Volunteer { get; set; } = null!;
    public Opportunity Opportunity { get; set; } = null!;
    public Application Application { get; set; } = null!;
    public OrganizationProfile? VerifiedByOrganization { get; set; }
}
