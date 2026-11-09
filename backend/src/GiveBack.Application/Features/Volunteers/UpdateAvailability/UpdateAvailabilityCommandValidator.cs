using FluentValidation;

namespace GiveBack.Application.Features.Volunteers.UpdateAvailability;

public sealed class UpdateAvailabilityCommandValidator : AbstractValidator<UpdateAvailabilityCommand>
{
    public UpdateAvailabilityCommandValidator()
    {
        RuleFor(x => x.Availability).NotNull();
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
