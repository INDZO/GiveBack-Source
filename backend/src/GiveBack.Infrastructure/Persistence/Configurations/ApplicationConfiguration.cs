using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class ApplicationConfiguration : IEntityTypeConfiguration<VolunteerApplication>
{
    public void Configure(EntityTypeBuilder<VolunteerApplication> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Message).HasMaxLength(1000);
        builder.Property(x => x.ApplicationType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.OpportunityId, x.VolunteerId })
            .IsUnique()
            .HasFilter("[VolunteerId] IS NOT NULL AND [Status] IN (1, 2, 4, 5)");
        builder.HasIndex(x => new { x.OpportunityId, x.TeamId })
            .IsUnique()
            .HasFilter("[TeamId] IS NOT NULL AND [Status] IN (1, 2, 4, 5)");
        builder.HasOne(x => x.Opportunity).WithMany(x => x.Applications).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Volunteer).WithMany(x => x.Applications).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team).WithMany(x => x.Applications).HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
    }
}
