using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.CancelOpportunity;

public sealed class CancelOpportunityCommandValidator : AbstractValidator<CancelOpportunityCommand>
{
    public CancelOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
