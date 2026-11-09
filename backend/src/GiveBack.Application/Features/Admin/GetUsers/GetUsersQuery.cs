using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.GetUsers;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;
