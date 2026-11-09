using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.RejectHourLog;

public sealed record RejectHourLogCommand(int Id) : IRequest<VolunteerHourLogDto>;
