using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerInterestConfiguration : IEntityTypeConfiguration<VolunteerInterest>
{
    public void Configure(EntityTypeBuilder<VolunteerInterest> builder)
    {
        builder.HasKey(x => new { x.VolunteerId, x.CauseId });
        builder.HasOne(x => x.Volunteer).WithMany(x => x.VolunteerInterests).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Cause).WithMany(x => x.VolunteerInterests).HasForeignKey(x => x.CauseId).OnDelete(DeleteBehavior.Restrict);
    }
}
