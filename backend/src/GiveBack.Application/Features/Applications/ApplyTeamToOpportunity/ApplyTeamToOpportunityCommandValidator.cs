using FluentValidation;

namespace GiveBack.Application.Features.Applications.ApplyTeamToOpportunity;

public sealed class ApplyTeamToOpportunityCommandValidator : AbstractValidator<ApplyTeamToOpportunityCommand>
{
    public ApplyTeamToOpportunityCommandValidator()
    {
        RuleFor(x => x.OpportunityId).GreaterThan(0);
        RuleFor(x => x.TeamId).GreaterThan(0);
        RuleFor(x => x.Message).MaximumLength(1000);
    }
}
