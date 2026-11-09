using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.DeclineInvitation;

public sealed class DeclineInvitationCommandValidator : AbstractValidator<DeclineInvitationCommand>
{
    public DeclineInvitationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
