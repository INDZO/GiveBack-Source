using GiveBack.Application.Common.Interfaces;
using GiveBack.Domain.Entities;
using GiveBack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GiveBack.Infrastructure.Repositories;

public sealed class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(GiveBackDbContext dbContext) : base(dbContext) { }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => DbSet.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdWithRefreshTokensAsync(int id, CancellationToken cancellationToken = default)
        => DbSet.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        => DbContext.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => DbSet.AnyAsync(u => u.Email == email, cancellationToken);
}
