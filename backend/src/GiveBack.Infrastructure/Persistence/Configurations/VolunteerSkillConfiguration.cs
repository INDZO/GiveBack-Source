using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class VolunteerSkillConfiguration : IEntityTypeConfiguration<VolunteerSkill>
{
    public void Configure(EntityTypeBuilder<VolunteerSkill> builder)
    {
        builder.HasKey(x => new { x.VolunteerId, x.SkillId });
        builder.HasOne(x => x.Volunteer).WithMany(x => x.VolunteerSkills).HasForeignKey(x => x.VolunteerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Skill).WithMany(x => x.VolunteerSkills).HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Restrict);
    }
}
