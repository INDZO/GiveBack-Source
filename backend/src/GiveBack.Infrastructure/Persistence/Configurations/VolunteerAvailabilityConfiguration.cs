using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerAvailabilityConfiguration : IEntityTypeConfiguration<VolunteerAvailability>
{
    public void Configure(EntityTypeBuilder<VolunteerAvailability> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DayOfWeek).HasConversion<int>();
        builder.ToTable(t => t.HasCheckConstraint("CK_VolunteerAvailability_Time", "[StartTime] < [EndTime]"));
        builder.HasOne(x => x.Volunteer).WithMany(x => x.Availabilities).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Cascade);
    }
}
