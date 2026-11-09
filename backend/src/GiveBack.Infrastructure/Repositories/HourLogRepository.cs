using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Repositories;

public sealed class HourLogRepository : Repository<VolunteerHourLog>, IHourLogRepository
{
    public HourLogRepository(GiveBackDbContext dbContext) : base(dbContext) { }

    public Task<VolunteerHourLog?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
        => DbSet
            .Include(h => h.Volunteer).ThenInclude(v => v.User)
            .Include(h => h.Opportunity).ThenInclude(o => o.Organization)
            .Include(h => h.Application)
            .Include(h => h.VerifiedByOrganization)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
}
