using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Organizations.GetMyOpportunities;

public sealed record GetMyOpportunitiesQuery : IRequest<IReadOnlyList<OpportunityDto>>;
