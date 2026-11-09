using FluentValidation;

namespace GiveBack.Application.Features.HourLogs.ApproveHourLog;

public sealed class ApproveHourLogCommandValidator : AbstractValidator<ApproveHourLogCommand>
{
    public ApproveHourLogCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
