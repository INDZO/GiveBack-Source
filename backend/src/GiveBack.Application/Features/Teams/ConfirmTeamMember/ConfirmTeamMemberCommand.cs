using GiveBack.Application.Features.Teams.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Teams.ConfirmTeamMember;

public sealed record ConfirmTeamMemberCommand(int TeamId, int MemberId) : IRequest<TeamMemberDto>;
