using FluentValidation;

namespace GiveBack.Application.Features.HourLogs.RejectHourLog;

public sealed class RejectHourLogCommandValidator : AbstractValidator<RejectHourLogCommand>
{
    public RejectHourLogCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
