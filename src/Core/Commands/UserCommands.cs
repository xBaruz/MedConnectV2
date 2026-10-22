namespace MedConnect.Core.Commands;

public record RegisterUserCommand(
    string Email, 
    string Password, 
    string Firstname, 
    string Lastname, 
    DateOnly DateBirth
);

public record LoginCommand(
    string Email, 
    string Password
);

public record RefreshTokenCommand(string RefreshToken);
