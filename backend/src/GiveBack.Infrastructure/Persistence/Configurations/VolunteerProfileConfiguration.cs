using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerProfileConfiguration : IEntityTypeConfiguration<VolunteerProfile>
{
    public void Configure(EntityTypeBuilder<VolunteerProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Bio).HasMaxLength(2000);
        builder.Property(x => x.TotalHours).HasPrecision(10, 2);
    }
}
