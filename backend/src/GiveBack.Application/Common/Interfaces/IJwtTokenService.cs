using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
}
