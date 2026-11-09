using FluentValidation;

namespace GiveBack.Application.Features.Leaderboard.GetYearlyLeaderboard;

public sealed class GetYearlyLeaderboardQueryValidator : AbstractValidator<GetYearlyLeaderboardQuery>
{
    public GetYearlyLeaderboardQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}
