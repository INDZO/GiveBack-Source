using FluentValidation;

namespace GiveBack.Application.Features.Teams.DeclineTeamMember;

public sealed class DeclineTeamMemberCommandValidator : AbstractValidator<DeclineTeamMemberCommand>
{
    public DeclineTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).GreaterThan(0);
        RuleFor(x => x.MemberId).GreaterThan(0);
    }
}
