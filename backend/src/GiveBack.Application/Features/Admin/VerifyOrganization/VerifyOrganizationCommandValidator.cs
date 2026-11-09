using FluentValidation;

namespace GiveBack.Application.Features.Admin.VerifyOrganization;

public sealed class VerifyOrganizationCommandValidator : AbstractValidator<VerifyOrganizationCommand>
{
    public VerifyOrganizationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
