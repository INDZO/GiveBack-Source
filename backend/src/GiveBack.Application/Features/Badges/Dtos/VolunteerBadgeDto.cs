namespace GiveBack.Application.Features.Badges.Dtos;

public sealed class VolunteerBadgeDto
{
    public int Id { get; set; }
    public int VolunteerId { get; set; }
    public int BadgeId { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string BadgeDescription { get; set; } = string.Empty;
    public DateTime AwardedAt { get; set; }
}
