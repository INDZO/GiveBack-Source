using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.ImpactReports.GetVolunteerAnnualImpact;

public sealed record GetVolunteerAnnualImpactQuery(int Year) : IRequest<VolunteerAnnualReportDto>;
