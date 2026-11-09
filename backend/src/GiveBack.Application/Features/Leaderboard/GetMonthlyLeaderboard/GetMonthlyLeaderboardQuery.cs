using GiveBack.Application.Features.Leaderboard.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Leaderboard.GetMonthlyLeaderboard;

public sealed record GetMonthlyLeaderboardQuery(int Month, int Year) : IRequest<IReadOnlyList<LeaderboardEntryDto>>;
