using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Policies;

public static class OpportunityTimeRules
{
    public static DateTime EndsAt(Opportunity opportunity)
        => opportunity.EndAt ?? opportunity.StartAt.AddHours((double)opportunity.DurationHours);

    public static bool HasEnded(Opportunity opportunity, DateTime? now = null)
        => EndsAt(opportunity) <= (now ?? DateTime.UtcNow);
}
