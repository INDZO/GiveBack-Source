using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.Role).HasConversion<int>();
        builder.HasOne(x => x.VolunteerProfile).WithOne(x => x.User).HasForeignKey<VolunteerProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.OrganizationProfile).WithOne(x => x.User).HasForeignKey<OrganizationProfile>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.RefreshTokens).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Notifications).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
