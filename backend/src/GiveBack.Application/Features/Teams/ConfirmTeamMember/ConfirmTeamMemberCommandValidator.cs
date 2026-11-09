using FluentValidation;

namespace GiveBack.Application.Features.Teams.ConfirmTeamMember;

public sealed class ConfirmTeamMemberCommandValidator : AbstractValidator<ConfirmTeamMemberCommand>
{
    public ConfirmTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).GreaterThan(0);
        RuleFor(x => x.MemberId).GreaterThan(0);
    }
}
