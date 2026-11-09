using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.StartAt).IsRequired();
        builder.Property(x => x.PeriodDescription).HasMaxLength(500);
        builder.Property(x => x.Location).HasMaxLength(300).IsRequired();
        builder.Property(x => x.DurationHours).HasPrecision(10, 2);
        builder.Property(x => x.Format).HasConversion<int>();
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Visibility).HasConversion<int>();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Opportunity_RequiredVolunteers", "[RequiredVolunteersCount] > 0");
            t.HasCheckConstraint("CK_Opportunity_DurationHours", "[DurationHours] > 0");
            t.HasCheckConstraint("CK_Opportunity_AcceptedCapacity", "[AcceptedVolunteersCount] <= [RequiredVolunteersCount]");
            t.HasCheckConstraint("CK_Opportunity_EndAfterStart", "[EndAt] IS NULL OR [EndAt] > [StartAt]");
        });
        builder.HasOne(x => x.Organization).WithMany(x => x.Opportunities).HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Cause).WithMany(x => x.Opportunities).HasForeignKey(x => x.CauseId).OnDelete(DeleteBehavior.Restrict);
    }
}
