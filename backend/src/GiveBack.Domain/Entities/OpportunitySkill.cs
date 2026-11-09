namespace GiveBack.Domain.Entities;

public class OpportunitySkill
{
    public int OpportunityId { get; set; }
    public int SkillId { get; set; }

    public Opportunity Opportunity { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
