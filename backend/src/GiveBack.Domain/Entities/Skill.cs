namespace GiveBack.Domain.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<VolunteerSkill> VolunteerSkills { get; set; } = new List<VolunteerSkill>();
    public ICollection<OpportunitySkill> OpportunitySkills { get; set; } = new List<OpportunitySkill>();
}
