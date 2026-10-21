using MedConnect.Core;
using MedConnect.Infrastructure.Security;
using MedConnect.Models;
using MedConnect.Repositories;
using MedConnect.Validation;

namespace MedConnect.Services;

public record RegisterUserCommand(
    string Email, 
    string Password, 
    string Firstname, 
    string Lastname, 
    DateOnly DateBirth
);

public record LoginCommand(
    string Email, 
    string Password);

public record AuthResponseDto(Guid UserId, string Token, string RefreshToken);

public record RefreshTokenCommand(string RefreshToken);

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterUserCommand command, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken ct = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken ct = default);
}

public class AuthService(
    IUserReadRepository readRepository,
    IUserWriteRepository writeRepository,
    IJwtTokenGenerator tokenGenerator,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly RuleSet<RegisterUserCommand> RegisterValidationRules = new RuleSet<RegisterUserCommand>()
        .AddRule(c => !string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains('@'), new RuleViolation("InvalidEmail", "Email jest niepoprawny."))
        .AddRule(c => !string.IsNullOrWhiteSpace(c.Password) && c.Password.Length >= 8, new RuleViolation("InvalidPassword", "Hasło musi mieć co najmniej 8 znaków."))
        .AddRule(c => !string.IsNullOrWhiteSpace(c.Firstname), new RuleViolation("FirstnameRequired", "Imię jest wymagane."))
        .AddRule(c => !string.IsNullOrWhiteSpace(c.Lastname), new RuleViolation("LastnameRequired", "Nazwisko jest wymagane."))
        .AddRule(c => c.DateBirth < DateOnly.FromDateTime(DateTime.UtcNow), new RuleViolation("InvalidBirthDate", "Data urodzenia musi być z przeszłości."));

    private static readonly RuleSet<LoginCommand> LoginValidationRules = new RuleSet<LoginCommand>()
        .AddRule(c => !string.IsNullOrWhiteSpace(c.Email) && c.Email.Contains('@'), new RuleViolation("InvalidEmail", "Email jest niepoprawny."))
        .AddRule(c => !string.IsNullOrWhiteSpace(c.Password), new RuleViolation("PasswordRequired", "Hasło jest wymagane."));

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterUserCommand command, CancellationToken ct = default)
    {
        var violations = RegisterValidationRules.Validate(command);
        if (violations.Any())
        {
            var message = string.Join("; ", violations.Select(v => v.Message));
            logger.LogWarning("Błąd walidacji rejestracji: {Violations}", message);
            return Result<AuthResponseDto>.Fail(new Error.Validation(message));
        }

        if (await readRepository.ExistsByEmailAsync(command.Email, ct))
        {
            logger.LogWarning("Koniec rejestracji - konflikt: Email {Email} istnieje w bazie", command.Email);
            return Result<AuthResponseDto>.Fail(new Error.Conflict("Użytkownik z tym adresem email już istnieje."));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
        var user = User.Create(command.Email, passwordHash, command.Firstname, command.Lastname, command.DateBirth);

        await writeRepository.AddAsync(user, ct);

        var jwtToken = tokenGenerator.GenerateToken(user);
        var refreshToken = RefreshToken.Create(user.Id);

        await writeRepository.AddRefreshTokenAsync(refreshToken, ct);
        await writeRepository.SaveChangesAsync(ct);

        logger.LogInformation("Użytkownik zarejestrowany poprawnie. ID: {UserId}", user.Id);

        return Result<AuthResponseDto>.Ok(new AuthResponseDto(user.Id, jwtToken, refreshToken.Token));
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginCommand command, CancellationToken ct = default)
    {
        var violations = LoginValidationRules.Validate(command);
        if (violations.Any())
        {
            var message = string.Join("; ", violations.Select(v => v.Message));
            logger.LogWarning("Błąd walidacji logowania: {Violations}", message);
            return Result<AuthResponseDto>.Fail(new Error.Validation(message));
        }

        var user = await readRepository.GetByEmailAsync(command.Email, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.Password))
        {
            logger.LogWarning("Logowanie nieudane dla emaila {Email}", command.Email);
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Nieprawidłowy email lub hasło."));
        }

        user.LastLoginAt = DateTime.UtcNow;
        writeRepository.Update(user);

        var jwtToken = tokenGenerator.GenerateToken(user);
        var refreshToken = RefreshToken.Create(user.Id);

        await writeRepository.AddRefreshTokenAsync(refreshToken, ct);
        await writeRepository.SaveChangesAsync(ct);

        logger.LogInformation("Użytkownik {UserId} zalogował się pomyślnie.", user.Id);

        return Result<AuthResponseDto>.Ok(new AuthResponseDto(user.Id, jwtToken, refreshToken.Token));
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var existingToken = await readRepository.GetRefreshTokenAsync(command.RefreshToken, ct);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Nieprawidłowy lub wygasły refresh token."));
        }

        existingToken.Revoke();

        var user = await readRepository.GetByIdAsync(existingToken.UserId, ct);
        if (user is null)
        {
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Użytkownik nie istnieje."));
        }

        var newJwtToken = tokenGenerator.GenerateToken(user);
        var newRefreshToken = RefreshToken.Create(user.Id);

        await writeRepository.AddRefreshTokenAsync(newRefreshToken, ct);
        await writeRepository.SaveChangesAsync(ct);

        return Result<AuthResponseDto>.Ok(new AuthResponseDto(user.Id, newJwtToken, newRefreshToken.Token));
    }
}