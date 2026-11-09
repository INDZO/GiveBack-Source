using AutoMapper;
using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Organizations.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, OrganizationProfileDto>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateMyProfileCommandHandler(ICurrentUserService currentUser, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OrganizationProfileDto> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Organizations.GetByUserIdAsync(_currentUser.RequireUserId(), cancellationToken)
            ?? throw new NotFoundException("Organization profile was not found.");
        var registrationNumber = request.RegistrationNumber.Trim();
        if (profile.IsVerified &&
            !string.Equals(profile.RegistrationNumber, registrationNumber, StringComparison.Ordinal))
        {
            throw new ConflictException("Verified organization cannot change registration number.");
        }
        var duplicateRegistrationNumber = await _unitOfWork.Organizations.Query()
            .AnyAsync(o => o.Id != profile.Id && o.RegistrationNumber == registrationNumber, cancellationToken);
        if (duplicateRegistrationNumber)
        {
            throw new ConflictException("Registration number is already registered.");
        }

        profile.Name = request.Name.Trim();
        profile.RegistrationNumber = registrationNumber;
        profile.Mission = request.Mission.Trim();
        profile.WorkArea = request.WorkArea.Trim();
        profile.Location = request.Location.Trim();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return _mapper.Map<OrganizationProfileDto>(profile);
    }
}
