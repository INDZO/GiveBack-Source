namespace GiveBack.Domain.Entities;

public class Cause
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<VolunteerInterest> VolunteerInterests { get; set; } = new List<VolunteerInterest>();
    public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}
