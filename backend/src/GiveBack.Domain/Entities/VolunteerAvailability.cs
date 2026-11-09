namespace GiveBack.Domain.Entities;

public class VolunteerAvailability
{
    public int Id { get; set; }
    public int VolunteerId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public VolunteerProfile Volunteer { get; set; } = null!;
}
