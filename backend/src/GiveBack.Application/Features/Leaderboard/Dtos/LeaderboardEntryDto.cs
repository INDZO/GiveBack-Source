namespace GiveBack.Application.Features.Leaderboard.Dtos;

public sealed class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public int VolunteerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public int NumberOfOpportunities { get; set; }
}
