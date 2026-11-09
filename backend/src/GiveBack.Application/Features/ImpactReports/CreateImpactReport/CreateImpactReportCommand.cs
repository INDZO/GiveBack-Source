using GiveBack.Application.Features.ImpactReports.Dtos;
using MediatR;

namespace GiveBack.Application.Features.ImpactReports.CreateImpactReport;

public sealed record CreateImpactReportCommand(int OpportunityId, int BeneficiariesHelpedCount, string Summary) : IRequest<ImpactReportDto>;
