using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.GetOpportunityById;

public sealed record GetOpportunityByIdQuery(int Id) : IRequest<OpportunityDetailsDto>;
