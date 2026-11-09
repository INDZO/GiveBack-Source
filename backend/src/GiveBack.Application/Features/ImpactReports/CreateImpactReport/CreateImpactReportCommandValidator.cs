using FluentValidation;

namespace GiveBack.Application.Features.ImpactReports.CreateImpactReport;

public sealed class CreateImpactReportCommandValidator : AbstractValidator<CreateImpactReportCommand>
{
    public CreateImpactReportCommandValidator()
    {
        RuleFor(x => x.OpportunityId).GreaterThan(0);
        RuleFor(x => x.BeneficiariesHelpedCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
    }
}
