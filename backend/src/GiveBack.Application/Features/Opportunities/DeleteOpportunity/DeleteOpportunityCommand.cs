using MediatR;

namespace GiveBack.Application.Features.Opportunities.DeleteOpportunity;

public sealed record DeleteOpportunityCommand(int Id) : IRequest;
