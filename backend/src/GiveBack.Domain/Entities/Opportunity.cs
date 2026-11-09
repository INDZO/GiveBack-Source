using GiveBack.Domain.Enums;

namespace GiveBack.Domain.Entities;

public class Opportunity
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int CauseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredVolunteersCount { get; set; }
    public int AcceptedVolunteersCount { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? PeriodDescription { get; set; }
    public string Location { get; set; } = string.Empty;
    public OpportunityFormat Format { get; set; }
    public decimal DurationHours { get; set; }
    public OpportunityType Type { get; set; }
    public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;
    public OpportunityVisibility Visibility { get; set; } = OpportunityVisibility.Public;
    public bool IsTeamBased { get; set; }
    public bool IsUrgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public OrganizationProfile Organization { get; set; } = null!;
    public Cause Cause { get; set; } = null!;
    public ICollection<OpportunitySkill> OpportunitySkills { get; set; } = new List<OpportunitySkill>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<OpportunityInvitation> Invitations { get; set; } = new List<OpportunityInvitation>();
    public ICollection<VolunteerHourLog> HourLogs { get; set; } = new List<VolunteerHourLog>();
    public ImpactReport? ImpactReport { get; set; }
}
