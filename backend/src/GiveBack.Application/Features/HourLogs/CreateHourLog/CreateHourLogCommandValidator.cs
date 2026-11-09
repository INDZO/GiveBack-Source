using FluentValidation;

namespace GiveBack.Application.Features.HourLogs.CreateHourLog;

public sealed class CreateHourLogCommandValidator : AbstractValidator<CreateHourLogCommand>
{
    public CreateHourLogCommandValidator()
    {
        RuleFor(x => x.ApplicationId).GreaterThan(0);
        RuleFor(x => x.WorkDate)
            .NotEmpty()
            .LessThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("Work date cannot be in the future.");
        RuleFor(x => x.SubmittedHours).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
    }
}
