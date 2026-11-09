using GiveBack.Application.Features.Opportunities.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Opportunities.GetMyInvitations;

public sealed record GetMyInvitationsQuery : IRequest<IReadOnlyList<OpportunityInvitationDto>>;
