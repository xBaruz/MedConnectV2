namespace MedConnect.Models;

public class User
{
    public Guid Id { get;  set; }
    public string Email { get;  set; } = string.Empty;
    public string Password { get;  set; } = string.Empty;
    public string Firstname { get;  set; } = string.Empty;
    public string Lastname { get;  set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateOnly DateBirth { get;  set; }
    public DateTime CreatedAt { get;  set; }
    public DateTime? UpdatedAt { get;  set; }
    public DateTime? LastLoginAt { get;  set; }
    public string? ProfileImage { get;  set; }

    private User(){}
    public User(
        Guid id, string email, string password, string firstname, string lastname, string role, DateOnly datebirth, DateTime createdat
    )
    {
        Id = id;
        Email = email;
        Password = password;
        Firstname = firstname;
        Lastname = lastname;
        Role = role;
        DateBirth = datebirth;
        CreatedAt = createdat;
    }

    public static User Create(
        string email, string password, string firstname, string lastname, DateOnly datebirth
    )
    {
        return new User(Guid.NewGuid(), email, password, firstname, lastname, Models.Role.Patient(), datebirth, DateTime.UtcNow);
    }

    private static IEnumerable<string> AvailableRoles {get; } = new[] {"admin", "patient", "doctor"};

}


public class Role
{
    public string Value { get; }

    public static IEnumerable<string> AvailableRoles { get; } = new[] { "admin", "patient", "doctor" };

    public Role(string value)
    {
        var normalized = value?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalized) || !AvailableRoles.Contains(normalized))
        {
            throw new ArgumentException($"Nieprawidłowa rola: '{value}'. Dopuszczalne wartości to: {string.Join(", ", AvailableRoles)}.");
        }

        Value = normalized;
    }

    public static Role Admin() => new("admin");
    public static Role Patient() => new("patient");
    public static Role Doctor() => new("doctor");

    public override string ToString() => Value;
    public static implicit operator string(Role role) => role.Value;
}