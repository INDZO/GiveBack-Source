using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.CreateHourLog;

public sealed record CreateHourLogCommand(int ApplicationId, DateTime WorkDate, decimal SubmittedHours, string Description) : IRequest<VolunteerHourLogDto>;
