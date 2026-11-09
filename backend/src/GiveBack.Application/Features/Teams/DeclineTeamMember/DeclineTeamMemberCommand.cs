using GiveBack.Application.Features.Teams.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Teams.DeclineTeamMember;

public sealed record DeclineTeamMemberCommand(int TeamId, int MemberId) : IRequest<TeamMemberDto>;
