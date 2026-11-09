using GiveBack.Application.Features.Certificates.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Certificates.GetMyCertificates;

public sealed record GetMyCertificatesQuery : IRequest<IReadOnlyList<CertificateDto>>;
