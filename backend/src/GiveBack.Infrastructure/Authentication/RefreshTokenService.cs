using System.Security.Cryptography;
using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace GiveBack.Infrastructure.Authentication;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IConfiguration _configuration;

    public RefreshTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public RefreshToken CreateRefreshToken(int userId)
    {
        var days = _configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7;
        var plainToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new RefreshToken
        {
            UserId = userId,
            PlainToken = plainToken,
            Token = HashToken(plainToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        };
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hash);
    }

    public void Revoke(RefreshToken refreshToken, string? replacedByToken = null)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReplacedByToken = replacedByToken is null ? null : HashToken(replacedByToken);
    }
}
