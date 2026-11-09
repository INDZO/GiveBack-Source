using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Lookups.GetCauses;

public sealed record GetCausesQuery : IRequest<IReadOnlyList<CauseDto>>;
