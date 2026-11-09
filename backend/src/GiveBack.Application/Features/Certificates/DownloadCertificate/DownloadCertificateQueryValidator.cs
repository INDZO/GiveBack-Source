using FluentValidation;

namespace GiveBack.Application.Features.Certificates.DownloadCertificate;

public sealed class DownloadCertificateQueryValidator : AbstractValidator<DownloadCertificateQuery>
{
    public DownloadCertificateQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
