using GiveBack.Application.Features.Badges.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Badges.GetBadges;

public sealed record GetBadgesQuery : IRequest<IReadOnlyList<BadgeDto>>;
