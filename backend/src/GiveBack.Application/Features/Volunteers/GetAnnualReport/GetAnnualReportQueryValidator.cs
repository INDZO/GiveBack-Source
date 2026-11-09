using FluentValidation;

namespace GiveBack.Application.Features.Volunteers.GetAnnualReport;

public sealed class GetAnnualReportQueryValidator : AbstractValidator<GetAnnualReportQuery>
{
    public GetAnnualReportQueryValidator()
    {
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
    }
}
