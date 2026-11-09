using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.UpdateInterests;

public sealed record UpdateInterestsCommand(IReadOnlyList<int> CauseIds) : IRequest<IReadOnlyList<CauseDto>>;
