using GiveBack.Domain.Entities;

namespace GiveBack.Application.Common.Interfaces;

public interface IOpportunityAccessPolicy
{
    Task<bool> CanViewAsync(Opportunity opportunity, CancellationToken cancellationToken = default);
}
