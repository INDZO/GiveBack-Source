using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;
