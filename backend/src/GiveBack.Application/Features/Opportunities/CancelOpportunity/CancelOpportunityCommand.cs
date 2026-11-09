using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.CancelOpportunity;

public sealed record CancelOpportunityCommand(int Id) : IRequest<OpportunityDetailsDto>;
