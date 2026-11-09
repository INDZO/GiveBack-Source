using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IHourLogRepository : IRepository<VolunteerHourLog>
{
    Task<VolunteerHourLog?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
}
