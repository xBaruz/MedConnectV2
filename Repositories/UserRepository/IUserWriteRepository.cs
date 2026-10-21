using MedConnect.Models;

namespace MedConnect.Repositories;

public interface IUserWriteRepository
{
    Task AddAsync(User user, CancellationToken ct = default);
    void Update(User user);
    void Remove(User user);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task RemoveExpiredRefreshTokensAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}