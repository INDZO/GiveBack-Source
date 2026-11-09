using GiveBack.Application.Features.Applications.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Applications.ApplyTeamToOpportunity;

public sealed record ApplyTeamToOpportunityCommand(int OpportunityId, int TeamId, string? Message) : IRequest<ApplicationDto>;
