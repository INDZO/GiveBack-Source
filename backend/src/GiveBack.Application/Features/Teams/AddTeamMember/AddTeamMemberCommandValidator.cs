using FluentValidation;

namespace GiveBack.Application.Features.Teams.AddTeamMember;

public sealed class AddTeamMemberCommandValidator : AbstractValidator<AddTeamMemberCommand>
{
    public AddTeamMemberCommandValidator()
    {
        RuleFor(x => x.TeamId).GreaterThan(0);
        RuleFor(x => x.VolunteerId).GreaterThan(0);
    }
}
