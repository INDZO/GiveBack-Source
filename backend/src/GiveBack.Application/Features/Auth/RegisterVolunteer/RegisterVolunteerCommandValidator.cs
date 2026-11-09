using FluentValidation;

namespace GiveBack.Application.Features.Auth.RegisterVolunteer;

public sealed class RegisterVolunteerCommandValidator : AbstractValidator<RegisterVolunteerCommand>
{
    public RegisterVolunteerCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Bio).MaximumLength(2000);
        RuleForEach(x => x.SkillIds).GreaterThan(0);
        RuleForEach(x => x.CauseIds).GreaterThan(0);
        RuleForEach(x => x.Availability).NotNull();
        RuleForEach(x => x.Availability)
            .Where(x => x is not null)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.DayOfWeek).IsInEnum();
                item.RuleFor(x => x.StartTime).LessThan(x => x.EndTime).WithMessage("Availability start time must be before end time.");
            });
    }
}
