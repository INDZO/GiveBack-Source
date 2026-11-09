using FluentValidation;

namespace GiveBack.Application.Features.Applications.MarkApplicationNoShow;

public sealed class MarkApplicationNoShowCommandValidator : AbstractValidator<MarkApplicationNoShowCommand>
{
    public MarkApplicationNoShowCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
