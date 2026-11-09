using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalHours).HasPrecision(10, 2);
        builder.Property(x => x.PdfUrl).HasMaxLength(500).IsRequired();
        builder.HasOne(x => x.Volunteer).WithMany(x => x.Certificates).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Restrict);
    }
}
