using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Common.Policies;
using GiveBack.Application.Features.Applications.Dtos;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Applications.MarkApplicationAttended;

public sealed class MarkApplicationAttendedCommandHandler : IRequestHandler<MarkApplicationAttendedCommand, ApplicationDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<MarkApplicationAttendedCommandHandler> _logger;

    public MarkApplicationAttendedCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper, ILogger<MarkApplicationAttendedCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApplicationDto> Handle(MarkApplicationAttendedCommand request, CancellationToken cancellationToken)
    {
        var organization = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var application = await _unitOfWork.Applications.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Application was not found.");
        if (application.Opportunity.OrganizationId != organization.Id)
        {
            throw new ForbiddenException("Only the owning organization can mark attended this application.");
        }
        if (application.Status != ApplicationStatus.Accepted)
        {
            throw new ConflictException("Invalid application status transition.");
        }
        if (!OpportunityTimeRules.HasEnded(application.Opportunity))
        {
            throw new ConflictException("Application cannot be marked attended before the opportunity ends.");
        }

        application.Status = ApplicationStatus.Attended;
        application.ReviewedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Application {ApplicationId} changed to {Status}", application.Id, application.Status);
        return _mapper.Map<ApplicationDto>(application);
    }
}
