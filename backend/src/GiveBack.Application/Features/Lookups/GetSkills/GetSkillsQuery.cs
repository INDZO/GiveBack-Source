using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Lookups.GetSkills;

public sealed record GetSkillsQuery : IRequest<IReadOnlyList<SkillDto>>;
