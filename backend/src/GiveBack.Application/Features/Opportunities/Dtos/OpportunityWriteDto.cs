using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Opportunities.Dtos;

public sealed class OpportunityWriteDto
{
    public int CauseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RequiredVolunteersCount { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? PeriodDescription { get; set; }
    public string Location { get; set; } = string.Empty;
    public OpportunityFormat Format { get; set; }
    public decimal DurationHours { get; set; }
    public OpportunityType Type { get; set; }
    public OpportunityVisibility Visibility { get; set; }
    public bool IsTeamBased { get; set; }
    public bool IsUrgent { get; set; }
    public IReadOnlyList<int>? SkillIds { get; set; }
}
