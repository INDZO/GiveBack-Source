namespace GiveBack.Application.Features.Admin.Dtos;

public sealed class AdminStatisticsDto
{
    public int UsersCount { get; set; }
    public int VolunteersCount { get; set; }
    public int OrganizationsCount { get; set; }
    public int VerifiedOrganizationsCount { get; set; }
    public int PendingOrganizationsCount { get; set; }
    public int OpportunitiesCount { get; set; }
    public int OpenOpportunitiesCount { get; set; }
    public int FilledOpportunitiesCount { get; set; }
    public int CancelledOpportunitiesCount { get; set; }
    public int CompletedOpportunitiesCount { get; set; }
    public int UrgentOpportunitiesCount { get; set; }
    public int ApplicationsCount { get; set; }
    public decimal TotalVerifiedHours { get; set; }
    public int CertificatesCount { get; set; }
}
