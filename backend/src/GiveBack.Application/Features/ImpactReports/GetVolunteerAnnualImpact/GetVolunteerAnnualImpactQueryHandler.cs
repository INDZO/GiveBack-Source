using GiveBack.Application.Features.Volunteers.Dtos;
using GiveBack.Application.Features.Volunteers.GetAnnualReport;
using MediatR;

namespace GiveBack.Application.Features.ImpactReports.GetVolunteerAnnualImpact;

public sealed class GetVolunteerAnnualImpactQueryHandler : IRequestHandler<GetVolunteerAnnualImpactQuery, VolunteerAnnualReportDto>
{
    private readonly IMediator _mediator;

    public GetVolunteerAnnualImpactQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<VolunteerAnnualReportDto> Handle(GetVolunteerAnnualImpactQuery request, CancellationToken cancellationToken)
    {
        return _mediator.Send(new GetAnnualReportQuery(request.Year), cancellationToken);
    }
}
