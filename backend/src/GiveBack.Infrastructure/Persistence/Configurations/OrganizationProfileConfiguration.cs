using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class OrganizationProfileConfiguration : IEntityTypeConfiguration<OrganizationProfile>
{
    public void Configure(EntityTypeBuilder<OrganizationProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RegistrationNumber).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.RegistrationNumber).IsUnique();
        builder.Property(x => x.Mission).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.WorkArea).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Location).HasMaxLength(300).IsRequired();
        builder.Property(x => x.VerificationRejectionReason).HasMaxLength(1000);
        builder.HasOne(x => x.VerifiedByAdmin)
            .WithMany(x => x.VerifiedOrganizations)
            .HasForeignKey(x => x.VerifiedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
