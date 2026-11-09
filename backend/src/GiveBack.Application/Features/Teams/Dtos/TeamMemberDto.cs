using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Teams.Dtos;

public sealed class TeamMemberDto
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int VolunteerId { get; set; }
    public string VolunteerName { get; set; } = string.Empty;
    public TeamMemberStatus Status { get; set; }
}
