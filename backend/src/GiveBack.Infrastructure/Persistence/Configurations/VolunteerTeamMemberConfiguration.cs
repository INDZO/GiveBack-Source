using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerTeamMemberConfiguration : IEntityTypeConfiguration<VolunteerTeamMember>
{
    public void Configure(EntityTypeBuilder<VolunteerTeamMember> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.TeamId, x.VolunteerId }).IsUnique();
        builder.HasOne(x => x.Team).WithMany(x => x.Members).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Volunteer).WithMany(x => x.TeamMemberships).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Restrict);
    }
}
