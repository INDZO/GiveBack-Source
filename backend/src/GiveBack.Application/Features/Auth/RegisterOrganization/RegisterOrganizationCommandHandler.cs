using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using GiveBack.Domain.Entities;
using GiveBack.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Application.Features.Auth.RegisterOrganization;

public sealed class RegisterOrganizationCommandHandler : IRequestHandler<RegisterOrganizationCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public RegisterOrganizationCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponseDto> Handle(RegisterOrganizationCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _unitOfWork.Users.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }
        var registrationNumber = request.RegistrationNumber.Trim();
        var registrationNumberExists = await _unitOfWork.Organizations.Query()
            .AnyAsync(o => o.RegistrationNumber == registrationNumber, cancellationToken);
        if (registrationNumberExists)
        {
            throw new ConflictException("Registration number is already registered.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordService.HashPassword(request.Password),
            Role = UserRole.Organization,
            IsActive = true,
            OrganizationProfile = new OrganizationProfile
            {
                Name = request.Name.Trim(),
                RegistrationNumber = registrationNumber,
                Mission = request.Mission.Trim(),
                WorkArea = request.WorkArea.Trim(),
                Location = request.Location.Trim(),
                IsVerified = false
            }
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshToken = _refreshTokenService.CreateRefreshToken(user.Id);
        await _unitOfWork.Repository<GiveBack.Domain.Entities.RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken.PlainToken,
            UserId = user.Id,
            Role = user.Role
        };
    }
}
