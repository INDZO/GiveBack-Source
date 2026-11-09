using GiveBack.Application.Common.Interfaces;
using GiveBack.Infrastructure.Persistence;
using GiveBack.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace GiveBack.Infrastructure.UnitOfWork;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly GiveBackDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        GiveBackDbContext dbContext,
        IUserRepository users,
        IOpportunityRepository opportunities,
        IApplicationRepository applications,
        IVolunteerRepository volunteers,
        IOrganizationRepository organizations,
        IHourLogRepository hourLogs)
    {
        _dbContext = dbContext;
        Users = users;
        Opportunities = opportunities;
        Applications = applications;
        Volunteers = volunteers;
        Organizations = organizations;
        HourLogs = hourLogs;
    }

    public IUserRepository Users { get; }
    public IOpportunityRepository Opportunities { get; }
    public IApplicationRepository Applications { get; }
    public IVolunteerRepository Volunteers { get; }
    public IOrganizationRepository Organizations { get; }
    public IHourLogRepository HourLogs { get; }

    public IRepository<T> Repository<T>() where T : class
    {
        if (_repositories.TryGetValue(typeof(T), out var repository))
        {
            return (IRepository<T>)repository;
        }

        var newRepository = new Repository<T>(_dbContext);
        _repositories[typeof(T)] = newRepository;
        return newRepository;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            return;
        }
        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}
