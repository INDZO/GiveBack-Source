using GiveBack.Domain.Enums;

namespace GiveBack.Domain.Entities;

public class Application
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int? VolunteerId { get; set; }
    public int? TeamId { get; set; }
    public string? Message { get; set; }
    public ApplicationType ApplicationType { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    public Opportunity Opportunity { get; set; } = null!;
    public VolunteerProfile? Volunteer { get; set; }
    public VolunteerTeam? Team { get; set; }
    public ICollection<VolunteerHourLog> HourLogs { get; set; } = new List<VolunteerHourLog>();
}
