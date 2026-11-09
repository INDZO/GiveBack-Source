using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Certificates.Dtos;
using GiveBack.Domain.Entities;
using MediatR;

namespace GiveBack.Application.Features.Certificates.DownloadCertificate;

public sealed class DownloadCertificateQueryHandler : IRequestHandler<DownloadCertificateQuery, CertificateDownloadDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICertificatePdfService _certificatePdfService;

    public DownloadCertificateQueryHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, ICertificatePdfService certificatePdfService)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _certificatePdfService = certificatePdfService;
    }

    public async Task<CertificateDownloadDto> Handle(DownloadCertificateQuery request, CancellationToken cancellationToken)
    {
        var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Volunteer profile was not found.");
        var certificate = await _unitOfWork.Repository<Certificate>().GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Certificate was not found.");
        if (certificate.VolunteerId != volunteer.Id)
        {
            throw new ForbiddenException("Certificate does not belong to the current volunteer.");
        }
        var file = await _certificatePdfService.OpenReadAsync(certificate.PdfUrl, cancellationToken);
        return new CertificateDownloadDto { Stream = file.Stream, ContentType = file.ContentType, FileName = file.FileName };
    }
}
