namespace GiveBack.Application.Common.Interfaces;

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IOpportunityRepository Opportunities { get; }
    IApplicationRepository Applications { get; }
    IVolunteerRepository Volunteers { get; }
    IOrganizationRepository Organizations { get; }
    IHourLogRepository HourLogs { get; }
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
