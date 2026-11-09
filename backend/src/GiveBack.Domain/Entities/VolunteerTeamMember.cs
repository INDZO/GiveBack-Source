using GiveBack.Domain.Enums;

namespace GiveBack.Domain.Entities;

public class VolunteerTeamMember
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public int VolunteerId { get; set; }
    public TeamMemberStatus Status { get; set; } = TeamMemberStatus.Pending;

    public VolunteerTeam Team { get; set; } = null!;
    public VolunteerProfile Volunteer { get; set; } = null!;
}
