using GiveBack.Application.Features.Certificates.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Certificates.GenerateCertificate;

public sealed record GenerateCertificateCommand(DateTime PeriodFrom, DateTime PeriodTo) : IRequest<CertificateDto>;
