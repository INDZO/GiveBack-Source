using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.GetPendingOrganizationHourLogs;

public sealed record GetPendingOrganizationHourLogsQuery : IRequest<IReadOnlyList<VolunteerHourLogDto>>;
