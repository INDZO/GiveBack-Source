using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Certificates.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Certificates.GenerateCertificate;

public sealed class GenerateCertificateCommandHandler : IRequestHandler<GenerateCertificateCommand, CertificateDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICertificatePdfService _certificatePdfService;
    private readonly IMapper _mapper;
    private readonly ILogger<GenerateCertificateCommandHandler> _logger;

    public GenerateCertificateCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, ICertificatePdfService certificatePdfService, IMapper mapper, ILogger<GenerateCertificateCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _certificatePdfService = certificatePdfService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CertificateDto> Handle(GenerateCertificateCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        string? generatedPdfUrl = null;
        try
        {
            var volunteer = await _unitOfWork.Volunteers.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
                ?? throw new NotFoundException("Volunteer profile was not found.");
            var logs = await _unitOfWork.HourLogs.Query()
                .Where(h => h.VolunteerId == volunteer.Id &&
                            (h.Status == HourLogStatus.Approved || h.Status == HourLogStatus.Corrected) &&
                            h.WorkDate >= request.PeriodFrom.Date && h.WorkDate <= request.PeriodTo.Date)
                .Include(h => h.Opportunity)
                .ThenInclude(o => o.Organization)
                .ToListAsync(cancellationToken);
            if (logs.Count == 0)
            {
                throw new ConflictException("No verified volunteer hours were found in the selected certificate period.");
            }

            var totalHours = logs.Sum(h => h.ApprovedHours ?? 0);
            var details = logs
                .Select(h => $"{h.WorkDate:yyyy-MM-dd}: {h.Opportunity.Title} ({h.Opportunity.Organization.Name}) - {(h.ApprovedHours ?? 0):0.##}h")
                .Distinct()
                .ToList();
            var pdfUrl = await _certificatePdfService.GenerateAsync($"{volunteer.FirstName} {volunteer.LastName}", request.PeriodFrom, request.PeriodTo, totalHours, details, cancellationToken);
            generatedPdfUrl = pdfUrl;
            var certificate = new Certificate
            {
                VolunteerId = volunteer.Id,
                PeriodFrom = request.PeriodFrom,
                PeriodTo = request.PeriodTo,
                TotalHours = totalHours,
                PdfUrl = pdfUrl
            };
            await _unitOfWork.Repository<Certificate>().AddAsync(certificate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Certificate {CertificateId} generated for volunteer {VolunteerId}", certificate.Id, volunteer.Id);
            return _mapper.Map<CertificateDto>(certificate);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            if (generatedPdfUrl is not null)
            {
                await _certificatePdfService.DeleteAsync(generatedPdfUrl, cancellationToken);
            }
            throw;
        }
    }
}
