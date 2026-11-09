using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Opportunities.Dtos;

public sealed class OpportunityInvitationDto
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime InvitedAt { get; set; }
    public DateTime StartAt { get; set; }
}
