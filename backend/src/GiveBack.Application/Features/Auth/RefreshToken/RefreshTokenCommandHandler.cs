using GiveBack.Application.Common.Exceptions;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Application.Features.Auth.Dtos;
using GiveBack.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Auth.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService, ILogger<RefreshTokenCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var currentToken = await _unitOfWork.Users.GetRefreshTokenAsync(_refreshTokenService.HashToken(request.RefreshToken), cancellationToken);
        if (currentToken is null || currentToken.User is null || !currentToken.User.IsActive || currentToken.IsExpired || currentToken.IsRevoked)
        {
            _logger.LogWarning("Rejected refresh token usage.");
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var newToken = _refreshTokenService.CreateRefreshToken(currentToken.UserId);
        _refreshTokenService.Revoke(currentToken, newToken.PlainToken);
        await _unitOfWork.Repository<GiveBack.Domain.Entities.RefreshToken>().AddAsync(newToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token rotated for user {UserId}", currentToken.UserId);

        return new AuthResponseDto
        {
            AccessToken = _jwtTokenService.GenerateAccessToken(currentToken.User),
            RefreshToken = newToken.PlainToken,
            UserId = currentToken.User.Id,
            Role = currentToken.User.Role
        };
    }
}
