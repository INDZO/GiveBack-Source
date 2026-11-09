using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.AcceptInvitation;

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
