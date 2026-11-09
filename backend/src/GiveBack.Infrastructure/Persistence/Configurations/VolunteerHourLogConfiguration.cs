using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerHourLogConfiguration : IEntityTypeConfiguration<VolunteerHourLog>
{
    public void Configure(EntityTypeBuilder<VolunteerHourLog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SubmittedHours).HasPrecision(10, 2);
        builder.Property(x => x.ApprovedHours).HasPrecision(10, 2);
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.WorkDate).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.ApplicationId, x.VolunteerId, x.WorkDate }).IsUnique();
        builder.HasOne(x => x.Volunteer).WithMany(x => x.HourLogs).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Opportunity).WithMany(x => x.HourLogs).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Application).WithMany(x => x.HourLogs).HasForeignKey(x => x.ApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.VerifiedByOrganization).WithMany(x => x.VerifiedHourLogs).HasForeignKey(x => x.VerifiedByOrganizationId).OnDelete(DeleteBehavior.Restrict);
    }
}
