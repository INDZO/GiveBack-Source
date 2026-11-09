using GiveBack.Application.Features.Organizations.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Organizations.GetStatistics;

public sealed record GetStatisticsQuery : IRequest<OrganizationStatisticsDto>;
