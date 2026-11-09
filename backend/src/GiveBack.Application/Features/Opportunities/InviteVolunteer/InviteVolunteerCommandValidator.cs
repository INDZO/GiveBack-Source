using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.InviteVolunteer;

public sealed class InviteVolunteerCommandValidator : AbstractValidator<InviteVolunteerCommand>
{
    public InviteVolunteerCommandValidator()
    {
        RuleFor(x => x.OpportunityId).GreaterThan(0);
        RuleFor(x => x.VolunteerId).GreaterThan(0);
    }
}
