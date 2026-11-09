using FluentValidation;

namespace GiveBack.Application.Features.Admin.UpdateCause;

public sealed class UpdateCauseCommandValidator : AbstractValidator<UpdateCauseCommand>
{
    public UpdateCauseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
