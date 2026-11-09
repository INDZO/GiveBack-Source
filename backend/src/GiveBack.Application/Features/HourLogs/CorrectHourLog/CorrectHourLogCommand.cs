using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.HourLogs.CorrectHourLog;

public sealed record CorrectHourLogCommand(int Id, decimal ApprovedHours) : IRequest<VolunteerHourLogDto>;
