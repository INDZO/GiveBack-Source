using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.GetUrgentOpportunities;

public sealed record GetUrgentOpportunitiesQuery : IRequest<IReadOnlyList<OpportunityDto>>;
