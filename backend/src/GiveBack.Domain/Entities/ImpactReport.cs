namespace GiveBack.Domain.Entities;

public class ImpactReport
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int VolunteersCount { get; set; }
    public decimal TotalHours { get; set; }
    public int BeneficiariesHelpedCount { get; set; }
    public string Summary { get; set; } = string.Empty;

    public Opportunity Opportunity { get; set; } = null!;
}
