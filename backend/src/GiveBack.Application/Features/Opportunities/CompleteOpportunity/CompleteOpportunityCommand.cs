using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.CompleteOpportunity;

public sealed record CompleteOpportunityCommand(int Id) : IRequest<OpportunityDetailsDto>;
