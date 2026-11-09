using GiveBack.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiveBack.Application.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, IRefreshTokenService refreshTokenService, ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _unitOfWork.Users.GetRefreshTokenAsync(_refreshTokenService.HashToken(request.RefreshToken), cancellationToken);
        if (token is not null && token.IsActive)
        {
            _refreshTokenService.Revoke(token);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Refresh token revoked for user {UserId}", token.UserId);
        }
    }
}
