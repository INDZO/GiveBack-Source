using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class CauseConfiguration : IEntityTypeConfiguration<Cause>
{
    public void Configure(EntityTypeBuilder<Cause> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
