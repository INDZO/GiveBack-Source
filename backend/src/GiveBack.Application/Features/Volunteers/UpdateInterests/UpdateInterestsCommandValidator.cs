using FluentValidation;

namespace GiveBack.Application.Features.Volunteers.UpdateInterests;

public sealed class UpdateInterestsCommandValidator : AbstractValidator<UpdateInterestsCommand>
{
    public UpdateInterestsCommandValidator()
    {
        RuleFor(x => x.CauseIds).NotNull();
        RuleForEach(x => x.CauseIds).GreaterThan(0);
    }
}
