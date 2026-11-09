using GiveBack.Application.Features.Auth.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Auth.RegisterVolunteer;

public sealed record RegisterVolunteerCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? Bio,
    IReadOnlyList<int>? SkillIds,
    IReadOnlyList<int>? CauseIds,
    IReadOnlyList<AvailabilityRequestDto>? Availability) : IRequest<AuthResponseDto>;
