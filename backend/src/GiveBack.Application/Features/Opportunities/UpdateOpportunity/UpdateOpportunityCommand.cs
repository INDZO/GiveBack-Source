using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.UpdateOpportunity;

public sealed record UpdateOpportunityCommand(int Id, OpportunityWriteDto Opportunity) : IRequest<OpportunityDetailsDto>;
