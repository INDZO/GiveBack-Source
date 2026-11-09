using GiveBack.Application.Features.Certificates.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Certificates.DownloadCertificate;

public sealed record DownloadCertificateQuery(int Id) : IRequest<CertificateDownloadDto>;
