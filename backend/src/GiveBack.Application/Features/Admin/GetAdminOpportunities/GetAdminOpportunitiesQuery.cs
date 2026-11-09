using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.GetAdminOpportunities;

public sealed record GetAdminOpportunitiesQuery : IRequest<IReadOnlyList<OpportunityDto>>;
