using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class ImpactReportConfiguration : IEntityTypeConfiguration<ImpactReport>
{
    public void Configure(EntityTypeBuilder<ImpactReport> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalHours).HasPrecision(10, 2);
        builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => x.OpportunityId).IsUnique();
        builder.HasOne(x => x.Opportunity).WithOne(x => x.ImpactReport).HasForeignKey<ImpactReport>(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
    }
}
