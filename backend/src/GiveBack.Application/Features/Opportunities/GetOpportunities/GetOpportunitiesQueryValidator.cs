using FluentValidation;

namespace GiveBack.Application.Features.Opportunities.GetOpportunities;

public sealed class GetOpportunitiesQueryValidator : AbstractValidator<GetOpportunitiesQuery>
{
    public GetOpportunitiesQueryValidator()
    {
        RuleFor(x => x.CauseId).GreaterThan(0).When(x => x.CauseId.HasValue);
        RuleFor(x => x.Location).MaximumLength(300);
        RuleFor(x => x.Format).IsInEnum().When(x => x.Format.HasValue);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.MinDurationHours).GreaterThan(0).When(x => x.MinDurationHours.HasValue);
        RuleFor(x => x.MaxDurationHours).GreaterThan(0).When(x => x.MaxDurationHours.HasValue);
        RuleFor(x => x)
            .Must(x => !x.MinDurationHours.HasValue || !x.MaxDurationHours.HasValue || x.MinDurationHours <= x.MaxDurationHours)
            .WithMessage("Minimum duration must be less than or equal to maximum duration.");
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleForEach(x => x.SkillIds).GreaterThan(0);
    }
}
