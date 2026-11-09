using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.VerifyOrganization;

public sealed record VerifyOrganizationCommand(int Id) : IRequest<OrganizationProfileDto>;
