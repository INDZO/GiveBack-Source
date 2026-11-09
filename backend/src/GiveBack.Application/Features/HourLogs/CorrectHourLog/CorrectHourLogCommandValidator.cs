using FluentValidation;

namespace GiveBack.Application.Features.HourLogs.CorrectHourLog;

public sealed class CorrectHourLogCommandValidator : AbstractValidator<CorrectHourLogCommand>
{
    public CorrectHourLogCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ApprovedHours).GreaterThan(0);
    }
}
