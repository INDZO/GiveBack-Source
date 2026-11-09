using GiveBack.Application.Features.Teams.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Teams.CreateTeam;

public sealed record CreateTeamCommand(string Name) : IRequest<VolunteerTeamDto>;
