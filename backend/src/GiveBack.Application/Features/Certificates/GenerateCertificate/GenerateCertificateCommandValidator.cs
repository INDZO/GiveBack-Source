using FluentValidation;

namespace GiveBack.Application.Features.Certificates.GenerateCertificate;

public sealed class GenerateCertificateCommandValidator : AbstractValidator<GenerateCertificateCommand>
{
    public GenerateCertificateCommandValidator()
    {
        RuleFor(x => x.PeriodFrom).NotEmpty();
        RuleFor(x => x.PeriodTo).NotEmpty();
        RuleFor(x => x.PeriodFrom).LessThanOrEqualTo(x => x.PeriodTo);
    }
}
