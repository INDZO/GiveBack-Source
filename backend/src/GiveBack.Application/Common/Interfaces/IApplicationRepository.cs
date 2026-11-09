using GiveBack.Domain.Entities;
using VolunteerApplication = GiveBack.Domain.Entities.Application;

namespace GiveBack.Application.Common.Interfaces;

public interface IApplicationRepository : IRepository<VolunteerApplication>
{
    Task<VolunteerApplication?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
}
