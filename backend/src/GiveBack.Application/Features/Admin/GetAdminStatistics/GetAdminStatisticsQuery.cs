using GiveBack.Application.Features.Admin.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Admin.GetAdminStatistics;

public sealed record GetAdminStatisticsQuery : IRequest<AdminStatisticsDto>;
