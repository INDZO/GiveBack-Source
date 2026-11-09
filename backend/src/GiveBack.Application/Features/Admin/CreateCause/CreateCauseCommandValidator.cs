using FluentValidation;

namespace GiveBack.Application.Features.Admin.CreateCause;

public sealed class CreateCauseCommandValidator : AbstractValidator<CreateCauseCommand>
{
    public CreateCauseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
