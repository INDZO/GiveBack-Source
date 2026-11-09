using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.DeactivateUser;

public sealed record DeactivateUserCommand(int Id) : IRequest<UserDto>;
