using FluentValidation;

namespace GiveBack.Application.Features.Applications.WithdrawApplication;

public sealed class WithdrawApplicationCommandValidator : AbstractValidator<WithdrawApplicationCommand>
{
    public WithdrawApplicationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
