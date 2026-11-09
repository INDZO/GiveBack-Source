using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IOrganizationRepository : IRepository<OrganizationProfile>
{
    Task<OrganizationProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<OrganizationProfile?> GetDetailsAsync(int organizationId, CancellationToken cancellationToken = default);
}
