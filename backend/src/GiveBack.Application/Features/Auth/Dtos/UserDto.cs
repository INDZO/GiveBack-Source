using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Auth.Dtos;

public sealed class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
