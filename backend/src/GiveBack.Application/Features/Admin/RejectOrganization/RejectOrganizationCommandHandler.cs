using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Admin.RejectOrganization;

public sealed class RejectOrganizationCommandHandler : IRequestHandler<RejectOrganizationCommand, OrganizationProfileDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RejectOrganizationCommandHandler> _logger;

    public RejectOrganizationCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper, ILogger<RejectOrganizationCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrganizationProfileDto> Handle(RejectOrganizationCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.RequireUserId();
        var organization = await _unitOfWork.Organizations.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        organization.IsVerified = false;
        organization.VerifiedByAdminId = adminUserId;
        organization.VerifiedAt = DateTime.UtcNow;
        organization.VerificationRejectionReason = request.Reason.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Organization {OrganizationId} processed by admin {AdminUserId}", organization.Id, adminUserId);
        return _mapper.Map<OrganizationProfileDto>(organization);
    }
}
