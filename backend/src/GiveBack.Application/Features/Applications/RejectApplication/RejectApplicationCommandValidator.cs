using FluentValidation;

namespace GiveBack.Application.Features.Applications.RejectApplication;

public sealed class RejectApplicationCommandValidator : AbstractValidator<RejectApplicationCommand>
{
    public RejectApplicationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
