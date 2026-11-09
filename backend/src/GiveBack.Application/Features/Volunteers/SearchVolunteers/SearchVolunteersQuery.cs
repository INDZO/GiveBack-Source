using MediatR;

namespace GiveBack.Application.Features.Volunteers.SearchVolunteers;

public sealed record SearchVolunteersQuery(string? Name, int? SkillId, int? CauseId) : IRequest<IReadOnlyList<VolunteerSearchResultDto>>;
