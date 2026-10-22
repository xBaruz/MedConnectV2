using MedConnect.Infrastructure.Database;
using MedConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Repositories;

public class UserWriteRepository(AppDbContext dbContext) : IUserWriteRepository
{
    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await dbContext.Users.AddAsync(user, ct);
    }

    public void Update(User user)
    {
        dbContext.Users.Update(user);
    }

    public void Remove(User user)
    {
        dbContext.Users.Remove(user);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await dbContext.Set<RefreshToken>().AddAsync(refreshToken, ct);
    }

    public async Task RemoveExpiredRefreshTokensAsync(CancellationToken ct = default)
    {
        var expiredTokens = await dbContext.Set<RefreshToken>()
            .Where(rt => rt.IsRevoked || rt.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        if (expiredTokens.Count != 0)
        {
            dbContext.Set<RefreshToken>().RemoveRange(expiredTokens);
            await dbContext.SaveChangesAsync(ct);
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task SetLastLoginAtAsync(Guid userId, DateTime time, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, ct);

        if (user is null)
            throw new InvalidOperationException("Użytkownik nie znaleziony.");

        user.LastLoginAt = time;
    }
}