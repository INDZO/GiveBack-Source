using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class OpportunityInvitationConfiguration : IEntityTypeConfiguration<OpportunityInvitation>
{
    public void Configure(EntityTypeBuilder<OpportunityInvitation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.OpportunityId, x.VolunteerId }).IsUnique();
        builder.HasOne(x => x.Opportunity).WithMany(x => x.Invitations).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Volunteer).WithMany(x => x.Invitations).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Restrict);
    }
}
