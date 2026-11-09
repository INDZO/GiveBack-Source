using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Organizations.UpdateMyProfile;

public sealed record UpdateMyProfileCommand(string Name, string RegistrationNumber, string Mission, string WorkArea, string Location) : IRequest<OrganizationProfileDto>;
