using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Admin.VerifyOrganization;

public sealed class VerifyOrganizationCommandHandler : IRequestHandler<VerifyOrganizationCommand, OrganizationProfileDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<VerifyOrganizationCommandHandler> _logger;

    public VerifyOrganizationCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper, ILogger<VerifyOrganizationCommandHandler> logger)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrganizationProfileDto> Handle(VerifyOrganizationCommand request, CancellationToken cancellationToken)
    {
        var adminUserId = _currentUser.RequireUserId();
        var organization = await _unitOfWork.Organizations.GetDetailsAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        organization.IsVerified = true;
        organization.VerifiedByAdminId = adminUserId;
        organization.VerifiedAt = DateTime.UtcNow;
        organization.VerificationRejectionReason = null;
        organization.User.IsActive = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Organization {OrganizationId} processed by admin {AdminUserId}", organization.Id, adminUserId);
        return _mapper.Map<OrganizationProfileDto>(organization);
    }
}
