using FluentValidation;

namespace GiveBack.Application.Features.Admin.CancelOpportunityAsAdmin;

public sealed class CancelOpportunityAsAdminCommandValidator : AbstractValidator<CancelOpportunityAsAdminCommand>
{
    public CancelOpportunityAsAdminCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
