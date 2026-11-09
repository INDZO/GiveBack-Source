using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Opportunities.Dtos;

public class OpportunityDto
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public int CauseId { get; set; }
    public string CauseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredVolunteersCount { get; set; }
    public int AcceptedVolunteersCount { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? PeriodDescription { get; set; }
    public string Location { get; set; } = string.Empty;
    public OpportunityFormat Format { get; set; }
    public decimal DurationHours { get; set; }
    public OpportunityType Type { get; set; }
    public OpportunityStatus Status { get; set; }
    public OpportunityVisibility Visibility { get; set; }
    public bool IsTeamBased { get; set; }
    public bool IsUrgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<int> SkillIds { get; set; } = Array.Empty<int>();
    public IReadOnlyList<SkillDto> Skills { get; set; } = Array.Empty<SkillDto>();
    public IReadOnlyList<string> SkillNames { get; set; } = Array.Empty<string>();

    // Capability flags for the owning organization's management view. They are
    // populated only by the organization "my opportunities" query (where the
    // related data is known); on public/detail projections they stay default and
    // are ignored by the UI. The UI uses these instead of guessing backend rules.
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanInvite { get; set; }
    public bool CanCancel { get; set; }
    public bool CanComplete { get; set; }
    public bool CanCreateImpactReport { get; set; }
    public bool HasImpactReport { get; set; }
    public int VerifiedHoursCount { get; set; }
}
