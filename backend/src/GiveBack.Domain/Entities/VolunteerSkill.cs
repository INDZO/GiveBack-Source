namespace GiveBack.Domain.Entities;

public class VolunteerSkill
{
    public int VolunteerId { get; set; }
    public int SkillId { get; set; }

    public VolunteerProfile Volunteer { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
