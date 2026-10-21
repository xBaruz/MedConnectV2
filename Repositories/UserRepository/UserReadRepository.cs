using MedConnect.Infrastructure.Database;
using MedConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace MedConnect.Repositories;

public class UserReadRepository(AppDbContext dbContext) : IUserReadRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, ct);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        return await dbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
    }
}