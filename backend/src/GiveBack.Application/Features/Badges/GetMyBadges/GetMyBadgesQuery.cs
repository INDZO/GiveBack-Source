using GiveBack.Application.Features.Badges.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Badges.GetMyBadges;

public sealed record GetMyBadgesQuery : IRequest<IReadOnlyList<VolunteerBadgeDto>>;
