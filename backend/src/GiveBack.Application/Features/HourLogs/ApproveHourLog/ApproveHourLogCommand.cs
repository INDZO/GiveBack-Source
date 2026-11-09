using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.ApproveHourLog;

public sealed record ApproveHourLogCommand(int Id) : IRequest<VolunteerHourLogDto>;
