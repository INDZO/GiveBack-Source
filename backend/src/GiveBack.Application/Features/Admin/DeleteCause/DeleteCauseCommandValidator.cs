using FluentValidation;

namespace GiveBack.Application.Features.Admin.DeleteCause;

public sealed class DeleteCauseCommandValidator : AbstractValidator<DeleteCauseCommand>
{
    public DeleteCauseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
