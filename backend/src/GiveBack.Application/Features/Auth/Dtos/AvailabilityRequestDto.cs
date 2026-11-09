namespace GiveBack.Application.Features.Auth.Dtos;

public sealed class AvailabilityRequestDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
