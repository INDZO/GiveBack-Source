using FluentValidation;

namespace GiveBack.Application.Features.Admin.DeactivateUser;

public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
