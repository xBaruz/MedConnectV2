using MedConnect.Core;
using MedConnect.Models;
using MedConnect.Repositories;

namespace MedConnect.Services;

public record GetUserCommand(
    Guid? Id,
    string? Email
);

public interface IUserService
{
    Task<Result<UserResponseDto>> GetUserAsync(GetUserCommand command, CancellationToken ct = default);
}

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

public class UserService(
    IUserReadRepository readRepository,
    IUserContext context,
    ILogger<UserService> logger) : IUserService
{
    public async Task<Result<UserResponseDto>> GetUserAsync(GetUserCommand command, CancellationToken ct = default)
    {
        User? user = null;

        if (command.Id.HasValue)
        {
            user = await readRepository.GetByIdAsync(command.Id.Value, ct);
        }
        else if (!string.IsNullOrEmpty(command.Email))
        {
            user = await readRepository.GetByEmailAsync(command.Email.Trim().ToLowerInvariant(), ct);
        }
        else
        {
            user = await readRepository.GetByIdAsync(context.Identity.Id, ct);
        }

        if(user is null)
        {
            logger.LogWarning("Nie znaleziono użytkownika dla kryteriów: Id={Id}, Email={Email}, ContextUserId={ContextId}", 
                command.Id, command.Email, context.Identity.Id);

            var searchedId = command.Id.HasValue ? command.Id.Value : context.Identity.Id;
                
            return Result<UserResponseDto>.Fail(new Error.NotFound("Użytkownik nie został odnaleziony.", searchedId));
        }

        return Result<UserResponseDto>.Ok(new UserResponseDto(
            user.Id,
            user.Email,
            user.Firstname,
            user.Lastname,
            user.Role,
            user.DateBirth,
            user.CreatedAt,
            user.LastLoginAt,
            user.ProfileImage
        ));
    }
}