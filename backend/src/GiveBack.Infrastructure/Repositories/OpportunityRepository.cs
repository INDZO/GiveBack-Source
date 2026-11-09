using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Repositories;

public sealed class OpportunityRepository : Repository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(GiveBackDbContext dbContext) : base(dbContext) { }

    public Task<Opportunity?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
        => DetailsQuery().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<Opportunity?> GetForApplicationAsync(int id, CancellationToken cancellationToken = default)
        => DetailsQuery().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    private IQueryable<Opportunity> DetailsQuery()
        => DbSet
            .Include(o => o.Organization)
            .Include(o => o.Cause)
            .Include(o => o.OpportunitySkills).ThenInclude(os => os.Skill)
            .Include(o => o.Invitations)
            .Include(o => o.HourLogs)
            .Include(o => o.ImpactReport)
            .Include(o => o.Applications).ThenInclude(a => a.Volunteer).ThenInclude(v => v!.User)
            .Include(o => o.Applications).ThenInclude(a => a.Team).ThenInclude(t => t!.CaptainVolunteer).ThenInclude(v => v.User)
            .Include(o => o.Applications).ThenInclude(a => a.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.Volunteer).ThenInclude(v => v.User);
}
