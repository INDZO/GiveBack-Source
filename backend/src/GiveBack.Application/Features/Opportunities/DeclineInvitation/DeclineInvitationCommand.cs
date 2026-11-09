using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.DeclineInvitation;

public sealed record DeclineInvitationCommand(int Id) : IRequest<OpportunityInvitationDto>;
