using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.ActivateUser;

public sealed record ActivateUserCommand(int Id) : IRequest<UserDto>;
