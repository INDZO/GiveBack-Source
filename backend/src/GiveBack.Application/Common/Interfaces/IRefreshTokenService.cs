using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    RefreshToken CreateRefreshToken(int userId);
    string HashToken(string token);
    void Revoke(RefreshToken refreshToken, string? replacedByToken = null);
}
