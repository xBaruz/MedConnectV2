namespace MedConnect.Models;

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Receptionist = "Receptionist";
    public const string Patient = "Patient";
    public const string Doctor = "Doctor";

    public static string ToName(Role role) => role switch
    {
        Role.Admin => Admin,
        Role.Receptionist => Receptionist,
        Role.Patient => Patient,
        Role.Doctor => Doctor,

        _ => throw new ArgumentOutOfRangeException(
            nameof(role),
            role,
            "Nieobsługiwana rola użytkownika."
        )
    };

    public static Role FromName(string name) => name switch
    {
        Admin => new Role.Admin(),
        Receptionist => new Role.Receptionist(),
        Patient => new Role.Patient(),
        Doctor => new Role.Doctor(),

        _ => throw new ArgumentException(
            "Nieznana nazwa roli użytkownika.",
            nameof(name))
    };
}