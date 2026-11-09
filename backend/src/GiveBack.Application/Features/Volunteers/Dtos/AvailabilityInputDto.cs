namespace GiveBack.Application.Features.Volunteers.Dtos;

public sealed class AvailabilityInputDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
