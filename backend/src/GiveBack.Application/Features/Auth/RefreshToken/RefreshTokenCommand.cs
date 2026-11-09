using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;
