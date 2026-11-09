using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.UpdateSkills;

public sealed record UpdateSkillsCommand(IReadOnlyList<int> SkillIds) : IRequest<IReadOnlyList<SkillDto>>;
