using GiveBack.Application.Features.Badges.Dtos;

namespace GiveBack.Application.Features.Volunteers.Dtos;

public sealed class VolunteerProfileDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal TotalHours { get; set; }
    public IReadOnlyList<int> SkillIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<SkillDto> Skills { get; set; } = Array.Empty<SkillDto>();
    public IReadOnlyList<string> SkillNames { get; set; } = Array.Empty<string>();
    public IReadOnlyList<int> CauseIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<CauseDto> Causes { get; set; } = Array.Empty<CauseDto>();
    public IReadOnlyList<string> CauseNames { get; set; } = Array.Empty<string>();
    public IReadOnlyList<VolunteerAvailabilityDto> Availability { get; set; } = Array.Empty<VolunteerAvailabilityDto>();
    public IReadOnlyList<VolunteerBadgeDto> Badges { get; set; } = Array.Empty<VolunteerBadgeDto>();
}
