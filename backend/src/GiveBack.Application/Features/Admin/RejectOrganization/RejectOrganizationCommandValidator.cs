using FluentValidation;

namespace GiveBack.Application.Features.Admin.RejectOrganization;

public sealed class RejectOrganizationCommandValidator : AbstractValidator<RejectOrganizationCommand>
{
    public RejectOrganizationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}
