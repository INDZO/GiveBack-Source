using FluentValidation;

namespace GiveBack.Application.Features.Applications.MarkApplicationAttended;

public sealed class MarkApplicationAttendedCommandValidator : AbstractValidator<MarkApplicationAttendedCommand>
{
    public MarkApplicationAttendedCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
