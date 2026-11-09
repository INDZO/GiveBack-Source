using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Repositories;

public sealed class OrganizationRepository : Repository<OrganizationProfile>, IOrganizationRepository
{
    public OrganizationRepository(GiveBackDbContext dbContext) : base(dbContext) { }

    public Task<OrganizationProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        => DbSet.Include(o => o.User).FirstOrDefaultAsync(o => o.UserId == userId, cancellationToken);

    public Task<OrganizationProfile?> GetDetailsAsync(int organizationId, CancellationToken cancellationToken = default)
        => DbSet.Include(o => o.User).Include(o => o.Opportunities).FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
}
