using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IVolunteerRepository : IRepository<VolunteerProfile>
{
    Task<VolunteerProfile?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<VolunteerProfile?> GetByUserIdWithDetailsAsync(int userId, CancellationToken cancellationToken = default);
    Task<VolunteerProfile?> GetDetailsAsync(int volunteerId, CancellationToken cancellationToken = default);
}
