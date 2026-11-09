using GiveBack.Application.Common.Interfaces;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly GiveBackDbContext DbContext;
    protected readonly DbSet<T> DbSet;

    public Repository(GiveBackDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<T>();
    }

    public IQueryable<T> Query() => DbSet.AsQueryable();

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => DbSet.Update(entity);

    public void Remove(T entity) => DbSet.Remove(entity);
}
