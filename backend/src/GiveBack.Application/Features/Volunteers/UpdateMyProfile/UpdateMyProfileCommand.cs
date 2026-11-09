using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(string FirstName, string LastName, string? Bio) : IRequest<VolunteerProfileDto>;
