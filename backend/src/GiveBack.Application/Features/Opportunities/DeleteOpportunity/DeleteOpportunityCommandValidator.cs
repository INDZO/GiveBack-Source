using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.DeleteOpportunity;

public sealed class DeleteOpportunityCommandValidator : AbstractValidator<DeleteOpportunityCommand>
{
    public DeleteOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
