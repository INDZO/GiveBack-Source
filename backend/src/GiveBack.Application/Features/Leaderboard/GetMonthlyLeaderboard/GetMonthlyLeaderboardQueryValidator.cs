using FluentValidation;

namespace GiveBack.Application.Features.Leaderboard.GetMonthlyLeaderboard;

public sealed class GetMonthlyLeaderboardQueryValidator : AbstractValidator<GetMonthlyLeaderboardQuery>
{
    public GetMonthlyLeaderboardQueryValidator()
    {
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}
