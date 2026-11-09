using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.GetAdminCauses;

public sealed record GetAdminCausesQuery : IRequest<IReadOnlyList<CauseDto>>;
