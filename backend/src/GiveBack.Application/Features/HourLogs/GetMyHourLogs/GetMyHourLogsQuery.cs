using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.GetMyHourLogs;

public sealed record GetMyHourLogsQuery : IRequest<IReadOnlyList<VolunteerHourLogDto>>;
