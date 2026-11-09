using FluentValidation;

namespace GiveBack.Application.Features.Volunteers.SearchVolunteers;

public sealed class SearchVolunteersQueryValidator : AbstractValidator<SearchVolunteersQuery>
{
    public SearchVolunteersQueryValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200);
        RuleFor(x => x.SkillId).GreaterThan(0).When(x => x.SkillId.HasValue);
        RuleFor(x => x.CauseId).GreaterThan(0).When(x => x.CauseId.HasValue);
    }
}
