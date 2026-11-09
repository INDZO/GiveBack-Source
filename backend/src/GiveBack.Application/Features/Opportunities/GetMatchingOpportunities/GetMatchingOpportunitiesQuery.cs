using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.GetMatchingOpportunities;

public sealed record GetMatchingOpportunitiesQuery : IRequest<IReadOnlyList<OpportunityDto>>;
