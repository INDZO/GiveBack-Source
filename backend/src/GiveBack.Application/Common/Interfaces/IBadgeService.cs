namespace GiveBack.Application.Common.Interfaces;

public interface IBadgeService
{
    Task EvaluateAndAwardBadgesAsync(int volunteerId, CancellationToken cancellationToken = default);
}
