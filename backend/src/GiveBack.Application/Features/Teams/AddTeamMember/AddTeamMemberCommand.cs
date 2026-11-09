using GiveBack.Application.Features.Teams.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Teams.AddTeamMember;

public sealed record AddTeamMemberCommand(int TeamId, int VolunteerId) : IRequest<TeamMemberDto>;
