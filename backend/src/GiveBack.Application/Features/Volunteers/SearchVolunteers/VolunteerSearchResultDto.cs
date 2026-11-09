namespace GiveBack.Application.Features.Volunteers.SearchVolunteers;

public sealed class VolunteerSearchResultDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public IReadOnlyList<string> SkillNames { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> CauseNames { get; set; } = Array.Empty<string>();
}
