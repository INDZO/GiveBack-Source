using GiveBack.Application.Features.Teams.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Teams.GetMyTeams;

public sealed record GetMyTeamsQuery : IRequest<IReadOnlyList<VolunteerTeamDto>>;
