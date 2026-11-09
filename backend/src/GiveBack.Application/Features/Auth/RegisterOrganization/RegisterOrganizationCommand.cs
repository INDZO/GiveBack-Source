using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Auth.RegisterOrganization;

public sealed record RegisterOrganizationCommand(
    string Email,
    string Password,
    string Name,
    string RegistrationNumber,
    string Mission,
    string WorkArea,
    string Location) : IRequest<AuthResponseDto>;
