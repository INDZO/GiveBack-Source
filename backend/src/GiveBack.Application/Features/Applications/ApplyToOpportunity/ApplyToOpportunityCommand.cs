using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.ApplyToOpportunity;

public sealed record ApplyToOpportunityCommand(int OpportunityId, string? Message) : IRequest<ApplicationDto>;
