using GiveBack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Infrastructure.Persistence;

public sealed class GiveBackDbContext : DbContext
{
    public GiveBackDbContext(DbContextOptions<GiveBackDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VolunteerProfile> VolunteerProfiles => Set<VolunteerProfile>();
    public DbSet<OrganizationProfile> OrganizationProfiles => Set<OrganizationProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<VolunteerSkill> VolunteerSkills => Set<VolunteerSkill>();
    public DbSet<Cause> Causes => Set<Cause>();
    public DbSet<VolunteerInterest> VolunteerInterests => Set<VolunteerInterest>();
    public DbSet<VolunteerAvailability> VolunteerAvailabilities => Set<VolunteerAvailability>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<OpportunitySkill> OpportunitySkills => Set<OpportunitySkill>();
    public DbSet<OpportunityInvitation> OpportunityInvitations => Set<OpportunityInvitation>();
    public DbSet<VolunteerApplication> Applications => Set<VolunteerApplication>();
    public DbSet<VolunteerTeam> VolunteerTeams => Set<VolunteerTeam>();
    public DbSet<VolunteerTeamMember> VolunteerTeamMembers => Set<VolunteerTeamMember>();
    public DbSet<VolunteerHourLog> VolunteerHourLogs => Set<VolunteerHourLog>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<ImpactReport> ImpactReports => Set<ImpactReport>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<VolunteerBadge> VolunteerBadges => Set<VolunteerBadge>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiveBackDbContext).Assembly);
    }
}
