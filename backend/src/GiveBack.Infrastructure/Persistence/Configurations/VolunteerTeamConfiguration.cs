using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerTeamConfiguration : IEntityTypeConfiguration<VolunteerTeam>
{
    public void Configure(EntityTypeBuilder<VolunteerTeam> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.HasOne(x => x.CaptainVolunteer).WithMany(x => x.CaptainedTeams).HasForeignKey(x => x.CaptainVolunteerId).OnDelete(DeleteBehavior.Restrict);
    }
}
