namespace GiveBack.Application.Features.Organizations.Dtos;

public sealed class OrganizationStatisticsDto
{
    public int TotalPublishedOpportunities { get; set; }
    public int TotalVolunteers { get; set; }
    public decimal TotalApprovedHours { get; set; }
    public int TotalBeneficiariesHelped { get; set; }
    public int CompletedActionsCount { get; set; }
}
