using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService, ILogger<LoginCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var refreshToken = _refreshTokenService.CreateRefreshToken(user.Id);
        await _unitOfWork.Repository<GiveBack.Domain.Entities.RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Successful login for user {UserId}", user.Id);

        return new AuthResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(user),
            RefreshToken = refreshToken.PlainToken,
            UserId = user.Id,
            Role = user.Role
        };
    }
}
