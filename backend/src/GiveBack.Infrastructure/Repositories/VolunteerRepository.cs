using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Repositories;

public sealed class VolunteerRepository : Repository<VolunteerProfile>, IVolunteerRepository
{
    public VolunteerRepository(GiveBackDbContext dbContext) : base(dbContext) { }

    public Task<VolunteerProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet.Include(v => v.User).FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

    public Task<VolunteerProfile?> GetByUserIdWithDetailsAsync(int userId, CancellationToken cancellationToken = default)
        => DetailsQuery().FirstOrDefaultAsync(v => v.UserId == userId, cancellationToken);

    public Task<VolunteerProfile?> GetDetailsAsync(int volunteerId, CancellationToken cancellationToken = default)
        => DetailsQuery().FirstOrDefaultAsync(v => v.Id == volunteerId, cancellationToken);

    private IQueryable<VolunteerProfile> DetailsQuery()
        => DbSet
            .Include(v => v.User)
            .Include(v => v.VolunteerSkills).ThenInclude(vs => vs.Skill)
            .Include(v => v.VolunteerInterests).ThenInclude(vi => vi.Cause)
            .Include(v => v.Availabilities)
            .Include(v => v.VolunteerBadges).ThenInclude(vb => vb.Badge);
}
