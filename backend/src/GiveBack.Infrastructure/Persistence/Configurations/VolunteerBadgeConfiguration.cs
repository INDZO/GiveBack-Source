using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerBadgeConfiguration : IEntityTypeConfiguration<VolunteerBadge>
{
    public void Configure(EntityTypeBuilder<VolunteerBadge> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.VolunteerId, x.BadgeId }).IsUnique();
        builder.HasOne(x => x.Volunteer).WithMany(x => x.VolunteerBadges).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Badge).WithMany(x => x.VolunteerBadges).HasForeignKey(x => x.BadgeId).OnDelete(DeleteBehavior.Restrict);
    }
}
