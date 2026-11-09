using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Organizations.GetImpact;

public sealed record GetImpactQuery : IRequest<OrganizationImpactDto>;
