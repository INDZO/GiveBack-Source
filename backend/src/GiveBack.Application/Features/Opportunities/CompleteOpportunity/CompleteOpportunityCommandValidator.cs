using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.CompleteOpportunity;

public sealed class CompleteOpportunityCommandValidator : AbstractValidator<CompleteOpportunityCommand>
{
    public CompleteOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
