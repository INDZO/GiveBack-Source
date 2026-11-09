using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Auth.Dtos;

public sealed class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public UserRole Role { get; set; }
}
