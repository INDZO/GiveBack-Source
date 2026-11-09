using FluentValidation;
using GiveBack.Domain.Enums;

namespace GiveBack.Application.Features.Opportunities.UpdateOpportunity;

public sealed class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Opportunity).NotNull();

        When(x => x.Opportunity is not null, () =>
        {
            RuleFor(x => x.Opportunity!.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Opportunity!.Description).NotEmpty().MaximumLength(4000);
            RuleFor(x => x.Opportunity!.CauseId).GreaterThan(0);
            RuleFor(x => x.Opportunity!.RequiredVolunteersCount).GreaterThan(0);
            RuleFor(x => x.Opportunity!.DurationHours).GreaterThan(0);
            RuleFor(x => x.Opportunity!.Format).IsInEnum();
            RuleFor(x => x.Opportunity!.Type).IsInEnum();
            RuleFor(x => x.Opportunity!.Visibility).IsInEnum();
            RuleFor(x => x.Opportunity!.StartAt).NotEmpty();
            RuleFor(x => x.Opportunity!.EndAt)
                .NotNull()
                .When(x => x.Opportunity!.Type is OpportunityType.Periodic or OpportunityType.LongTerm)
                .WithMessage("End date is required for periodic and long-term opportunities.");
            RuleFor(x => x.Opportunity!.EndAt)
                .GreaterThan(x => x.Opportunity!.StartAt)
                .When(x => x.Opportunity!.EndAt.HasValue);
            RuleFor(x => x.Opportunity!.PeriodDescription).MaximumLength(500);
            RuleFor(x => x.Opportunity!.Location)
                .NotEmpty()
                .MaximumLength(300)
                .When(x => x.Opportunity!.Format == OpportunityFormat.OnSite);
            RuleForEach(x => x.Opportunity!.SkillIds).GreaterThan(0);
        });
    }
}
