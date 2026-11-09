namespace GiveBack.Application.Features.Organizations.Dtos;

public sealed class OrganizationImpactDto
{
    public decimal TotalHours { get; set; }
    public int VolunteersCount { get; set; }
    public int BeneficiariesHelpedCount { get; set; }
    public int ReportsCount { get; set; }
}
