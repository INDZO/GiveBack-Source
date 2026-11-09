using GiveBack.Application.Features.Leaderboard.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Leaderboard.GetYearlyLeaderboard;

public sealed record GetYearlyLeaderboardQuery(int Year) : IRequest<IReadOnlyList<LeaderboardEntryDto>>;
