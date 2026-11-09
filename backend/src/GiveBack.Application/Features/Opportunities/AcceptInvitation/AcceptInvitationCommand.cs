using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.AcceptInvitation;

public sealed record AcceptInvitationCommand(int Id) : IRequest<OpportunityInvitationDto>;
