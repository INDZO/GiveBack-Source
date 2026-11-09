using FluentValidation;

namespace GiveBack.Application.Features.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
