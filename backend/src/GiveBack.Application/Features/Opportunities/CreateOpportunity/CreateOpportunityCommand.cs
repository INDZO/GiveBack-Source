using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.CreateOpportunity;

public sealed record CreateOpportunityCommand(OpportunityWriteDto Opportunity) : IRequest<OpportunityDetailsDto>;
