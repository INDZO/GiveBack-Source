using GiveBack.Application.Features.HourLogs.Dtos;
using MediatR;

namespace GiveBack.Application.Features.Volunteers.GetMyHours;

public sealed record GetMyHoursQuery : IRequest<IReadOnlyList<VolunteerHourLogDto>>;
