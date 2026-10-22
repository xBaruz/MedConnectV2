namespace MedConnect.Core.Dtos;

public record AuthResponseDto(Guid UserId, string Token, string RefreshToken);


public record UserResponseDto(
    Guid Id, 
    string Email, 
    string Firstname, 
    string Lastname, 
    string Role, 
    DateOnly DateBirth, 
    DateTime CreatedAt, 
    DateTime? LastLoginAt, 
    string? ProfileImage
);