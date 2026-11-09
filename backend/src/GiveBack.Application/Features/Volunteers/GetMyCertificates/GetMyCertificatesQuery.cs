using GiveBack.Application.Features.Certificates.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.GetMyCertificates;

public sealed record GetMyCertificatesQuery : IRequest<IReadOnlyList<CertificateDto>>;
