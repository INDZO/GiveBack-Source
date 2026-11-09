using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.GetPendingOrganizations;

public sealed record GetPendingOrganizationsQuery : IRequest<IReadOnlyList<OrganizationProfileDto>>;
