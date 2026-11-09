namespace GiveBack.Application.Features.ImpactReports.Dtos;

public sealed class ImpactReportDto
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public int VolunteersCount { get; set; }
    public decimal TotalHours { get; set; }
    public int BeneficiariesHelpedCount { get; set; }
    public string Summary { get; set; } = string.Empty;
}
