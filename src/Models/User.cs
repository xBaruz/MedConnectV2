namespace MedConnect.Models;

public class User
{
    public Guid Id { get;  set; }
    public string Email { get;  set; } = string.Empty;
    public string Password { get;  set; } = string.Empty;
    public string Firstname { get; set; } = string.Empty;
    public string Lastname { get; set; } = string.Empty;
    public Role Role { get; set; } = new Role.Patient();
    public DateOnly DateBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ProfileImage { get; set; }

    private User() { }

    public User(
        Guid id, 
        string email, 
        string password, 
        string firstname, 
        string lastname, 
        Role role, 
        DateOnly datebirth, 
        DateTime createdat
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
        string email, 
        string password, 
        string firstname, 
        string lastname, 
        DateOnly datebirth
    )
    {
        return new User(
            Guid.NewGuid(), 
            email, 
            password, 
            firstname, 
            lastname, 
            new Role.Patient(), 
            datebirth, 
            DateTime.UtcNow
        );
    }
}

public abstract record Role
{
    private Role() { }

    public sealed record Admin : Role;
    public sealed record Receptionist : Role;
    public sealed record Patient : Role;
    public sealed record Doctor : Role;
}