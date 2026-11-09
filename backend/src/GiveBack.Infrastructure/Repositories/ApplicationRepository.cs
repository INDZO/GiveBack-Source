using GiveBack.Application.Common.Interfaces;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Infrastructure.Repositories;

public sealed class ApplicationRepository : Repository<VolunteerApplication>, IApplicationRepository
{
    public ApplicationRepository(GiveBackDbContext dbContext) : base(dbContext) { }

    public Task<VolunteerApplication?> GetDetailsAsync(int id, CancellationToken cancellationToken = default)
        => DbSet
            .Include(a => a.Opportunity).ThenInclude(o => o.Organization)
            .Include(a => a.Volunteer).ThenInclude(v => v!.User)
            .Include(a => a.Team).ThenInclude(t => t!.CaptainVolunteer).ThenInclude(v => v.User)
            .Include(a => a.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.Volunteer).ThenInclude(v => v.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
}
