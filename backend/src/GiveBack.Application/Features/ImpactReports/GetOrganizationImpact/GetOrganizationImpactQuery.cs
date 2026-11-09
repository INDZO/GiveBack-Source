using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.ImpactReports.GetOrganizationImpact;

public sealed record GetOrganizationImpactQuery : IRequest<OrganizationImpactDto>;
