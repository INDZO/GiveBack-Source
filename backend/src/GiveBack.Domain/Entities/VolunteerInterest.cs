namespace GiveBack.Domain.Entities;

public class VolunteerInterest
{
    public int VolunteerId { get; set; }
    public int CauseId { get; set; }

    public VolunteerProfile Volunteer { get; set; } = null!;
    public Cause Cause { get; set; } = null!;
}
