namespace GiveBack.Application.Features.Teams.Dtos;

public sealed class VolunteerTeamDto
{
    public int Id { get; set; }
    public int CaptainVolunteerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<TeamMemberDto> Members { get; set; } = Array.Empty<TeamMemberDto>();
}
