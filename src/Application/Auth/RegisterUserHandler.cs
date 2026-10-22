using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Core.Dtos;
using MedConnect.Infrastructure.Security;
using MedConnect.Models;
using MedConnect.Repositories;

namespace MedConnect.Application.Auth;

public sealed class RegisterUserHandler(
    IUserReadRepository readRepository,
    IUserWriteRepository writeRepository,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator)
    : ICommandHandler<RegisterUserCommand, AuthResponseDto>
{
    public async Task<Result<AuthResponseDto>> HandleAsync(RegisterUserCommand command, CancellationToken ct = default)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var emailExists = await readRepository.ExistsByEmailAsync(normalizedEmail,ct);

        if (emailExists)
            return Result<AuthResponseDto>.Fail(new Error.Conflict("Użytkownik z tym adresem email już istnieje."));

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

        var user = User.Create(
            normalizedEmail,
            passwordHash,
            command.Firstname.Trim(),
            command.Lastname.Trim(),
            command.DateBirth);

        await writeRepository.AddAsync(user, ct);

        var jwtToken = tokenGenerator.GenerateToken(user);
        var rawRefreshToken = refreshTokenGenerator.Generate();
        var refreshTokenHash = refreshTokenGenerator.ComputeHash(rawRefreshToken);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash);

        await writeRepository.AddRefreshTokenAsync(refreshToken, ct);
        await writeRepository.SaveChangesAsync(ct);

        var response = new AuthResponseDto(
            user.Id,
            jwtToken,
            rawRefreshToken
        );

        return Result<AuthResponseDto>.Ok(response);
    }
}