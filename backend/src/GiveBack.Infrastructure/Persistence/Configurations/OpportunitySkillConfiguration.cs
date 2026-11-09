using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveBack.Infrastructure.Persistence.Configurations;

public sealed class OpportunitySkillConfiguration : IEntityTypeConfiguration<OpportunitySkill>
{
    public void Configure(EntityTypeBuilder<OpportunitySkill> builder)
    {
        builder.HasKey(x => new { x.OpportunityId, x.SkillId });
        builder.HasOne(x => x.Opportunity).WithMany(x => x.OpportunitySkills).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Skill).WithMany(x => x.OpportunitySkills).HasForeignKey(x => x.SkillId).OnDelete(DeleteBehavior.Restrict);
    }
}
