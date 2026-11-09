using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<VolunteerProfileDto>;
