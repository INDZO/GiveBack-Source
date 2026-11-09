using FluentValidation;

namespace GiveBack.Application.Features.Admin.ActivateUser;

public sealed class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
