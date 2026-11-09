using GiveBack.Domain.Enums;

namespace GiveBack.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public VolunteerProfile? VolunteerProfile { get; set; }
    public OrganizationProfile? OrganizationProfile { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<OrganizationProfile> VerifiedOrganizations { get; set; } = new List<OrganizationProfile>();
}
