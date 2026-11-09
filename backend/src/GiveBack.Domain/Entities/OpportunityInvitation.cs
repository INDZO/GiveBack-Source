using GiveBack.Domain.Enums;

namespace GiveBack.Domain.Entities;

public class OpportunityInvitation
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public int VolunteerId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

    public Opportunity Opportunity { get; set; } = null!;
    public VolunteerProfile Volunteer { get; set; } = null!;
}
