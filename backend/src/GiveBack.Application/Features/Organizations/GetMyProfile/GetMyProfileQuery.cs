using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Organizations.GetMyProfile;

public sealed record GetMyProfileQuery : IRequest<OrganizationProfileDto>;
