namespace GiveBack.Domain.Entities;

public class VolunteerBadge
{
    public int Id { get; set; }
    public int VolunteerId { get; set; }
    public int BadgeId { get; set; }
    public DateTime AwardedAt { get; set; } = DateTime.UtcNow;

    public VolunteerProfile Volunteer { get; set; } = null!;
    public Badge Badge { get; set; } = null!;
}
