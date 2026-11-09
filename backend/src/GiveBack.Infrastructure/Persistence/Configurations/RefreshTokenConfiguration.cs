using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.PlainToken);
        builder.Property(x => x.Token).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => x.Token).IsUnique();
        builder.Property(x => x.ReplacedByToken).HasMaxLength(512);
    }
}
