namespace MedConnect.Models;

public class User
{
    public Guid Id { get;  set; }
    public string Email { get;  set; } = string.Empty;
    public string Password { get;  set; } = string.Empty;
    public string Firstname { get;  set; } = string.Empty;
    public string Lastname { get;  set; } = string.Empty;
    public DateOnly DateBirth { get;  set; }
    public DateTime CreatedAt { get;  set; }
    public DateTime? UpdatedAt { get;  set; }
    public DateTime? LastLoginAt { get;  set; }
    public bool IsActive { get;  set; }
    public string? ProfileImage { get;  set; }

    private User(){}
    public User(
        Guid id, string email, string password, string firstname, string lastname, DateOnly datebirth, DateTime createdat
    )
    {
        Id = id;
        Email = email;
        Password = password;
        Firstname = firstname;
        Lastname = lastname;
        DateBirth = datebirth;
        CreatedAt = createdat;
    }

    public static User Create(
        string email, string password, string firstname, string lastname, DateOnly datebirth
    )
    {
        return new User(Guid.NewGuid(), email, password, firstname, lastname, datebirth, DateTime.UtcNow);
    }

    

}