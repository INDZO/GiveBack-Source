using MediatR;

namespace GiveBack.Application.Features.Auth.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
