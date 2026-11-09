using GiveBack.Application.Features.ImpactReports.Dtos;
using MediatR;

namespace GiveBack.Application.Features.ImpactReports.GetOpportunityImpactReport;

public sealed record GetOpportunityImpactReportQuery(int OpportunityId) : IRequest<ImpactReportDto>;
