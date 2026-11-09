using FluentValidation;

namespace GiveBack.Application.Features.Applications.AcceptApplication;

public sealed class AcceptApplicationCommandValidator : AbstractValidator<AcceptApplicationCommand>
{
    public AcceptApplicationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
