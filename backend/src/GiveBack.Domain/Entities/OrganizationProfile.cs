namespace GiveBack.Domain.Entities;

public class OrganizationProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Mission { get; set; } = string.Empty;
    public string WorkArea { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int? VerifiedByAdminId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationRejectionReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public User? VerifiedByAdmin { get; set; }
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public ICollection<VolunteerHourLog> VerifiedHourLogs { get; set; } = new List<VolunteerHourLog>();
}
