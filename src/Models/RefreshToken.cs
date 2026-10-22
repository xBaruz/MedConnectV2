namespace MedConnect.Models;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    private RefreshToken(){}

    public static RefreshToken Create(Guid userId, string tokenHash, int lifetimeDays = 7)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Id użytkownika nie może być puste.", nameof(userId));

        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        if (tokenHash.Length != 64)
            throw new ArgumentException("Skrót tokenu musi mieć 64 znaki.", nameof(tokenHash));

        if (lifetimeDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(lifetimeDays));

        var now = DateTime.UtcNow;

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            UserId = userId,
            CreatedAt = now,
            ExpiresAt = now.AddDays(lifetimeDays),
            IsRevoked = false
        };
    }

    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public void Revoke()
    {
        IsRevoked = true;
    }
}