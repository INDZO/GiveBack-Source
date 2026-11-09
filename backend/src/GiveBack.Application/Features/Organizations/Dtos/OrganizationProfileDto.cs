namespace GiveBack.Application.Features.Organizations.Dtos;

public sealed class OrganizationProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Mission { get; set; } = string.Empty;
    public string WorkArea { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int? VerifiedByAdminId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? VerificationRejectionReason { get; set; }
}
