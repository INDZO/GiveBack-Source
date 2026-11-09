using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Applications.Dtos;

public sealed class ApplicationDto
{
    public int Id { get; set; }
    public int OpportunityId { get; set; }
    public string OpportunityTitle { get; set; } = string.Empty;
    public int? VolunteerId { get; set; }
    public string? VolunteerName { get; set; }
    public int? TeamId { get; set; }
    public string? TeamName { get; set; }
    public string? Message { get; set; }
    public ApplicationType ApplicationType { get; set; }
    public ApplicationStatus Status { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    // Opportunity timing, exposed so the UI can tell whether attendance may be
    // marked yet (Attended/NoShow are only valid once the opportunity has ended,
    // matching the backend rule in Mark*CommandHandler).
    public DateTime OpportunityStartAt { get; set; }
    public DateTime? OpportunityEndAt { get; set; }
    public decimal OpportunityDurationHours { get; set; }
    public OpportunityStatus OpportunityStatus { get; set; }
}
