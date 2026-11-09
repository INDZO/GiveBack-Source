using FluentValidation;

namespace GiveBack.Application.Features.Applications.ApplyToOpportunity;

public sealed class ApplyToOpportunityCommandValidator : AbstractValidator<ApplyToOpportunityCommand>
{
    public ApplyToOpportunityCommandValidator()
    {
        RuleFor(x => x.OpportunityId).GreaterThan(0);
        RuleFor(x => x.Message).MaximumLength(1000);
    }
}
