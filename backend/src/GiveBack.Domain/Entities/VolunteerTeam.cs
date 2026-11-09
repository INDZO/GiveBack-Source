namespace GiveBack.Domain.Entities;

public class VolunteerTeam
{
    public int Id { get; set; }
    public int CaptainVolunteerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public VolunteerProfile CaptainVolunteer { get; set; } = null!;
    public ICollection<VolunteerTeamMember> Members { get; set; } = new List<VolunteerTeamMember>();
    public ICollection<Application> Applications { get; set; } = new List<Application>();
}
