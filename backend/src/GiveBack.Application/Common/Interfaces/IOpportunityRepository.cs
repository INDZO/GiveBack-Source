using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IOpportunityRepository : IRepository<Opportunity>
{
    Task<Opportunity?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<Opportunity?> GetForApplicationAsync(int id, CancellationToken cancellationToken = default);
}
