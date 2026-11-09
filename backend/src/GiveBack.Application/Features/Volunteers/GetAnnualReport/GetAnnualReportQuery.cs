using GiveBack.Application.Features.Volunteers.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.GetAnnualReport;

public sealed record GetAnnualReportQuery(int Year) : IRequest<VolunteerAnnualReportDto>;
