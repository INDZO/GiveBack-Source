namespace GiveBack.Domain.Entities;

public class VolunteerProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal TotalHours { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<VolunteerSkill> VolunteerSkills { get; set; } = new List<VolunteerSkill>();
    public ICollection<VolunteerInterest> VolunteerInterests { get; set; } = new List<VolunteerInterest>();
    public ICollection<VolunteerAvailability> Availabilities { get; set; } = new List<VolunteerAvailability>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
    public ICollection<VolunteerTeam> CaptainedTeams { get; set; } = new List<VolunteerTeam>();
    public ICollection<VolunteerTeamMember> TeamMemberships { get; set; } = new List<VolunteerTeamMember>();
    public ICollection<VolunteerHourLog> HourLogs { get; set; } = new List<VolunteerHourLog>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<VolunteerBadge> VolunteerBadges { get; set; } = new List<VolunteerBadge>();
    public ICollection<OpportunityInvitation> Invitations { get; set; } = new List<OpportunityInvitation>();
}
