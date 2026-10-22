using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Core.Dtos;
using MedConnect.Infrastructure.Security;
using MedConnect.Models;
using MedConnect.Repositories;

namespace MedConnect.Application.Auth;

public sealed class LoginHandler(
    IUserReadRepository readRepository,
    IUserWriteRepository writeRepository,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator)
    : ICommandHandler<LoginCommand, AuthResponseDto>
{
    public async Task<Result<AuthResponseDto>> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var user = await readRepository.GetByEmailAsync(command.Email, ct);

        if (user is null)
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Nieprawidłowy email lub hasło."));

        var passwordIsCorrect = BCrypt.Net.BCrypt.Verify(command.Password, user.Password);

        if (!passwordIsCorrect)
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Nieprawidłowy email lub hasło."));

        await writeRepository.SetLastLoginAtAsync(user.Id,DateTime.UtcNow,ct);

        var jwtToken = tokenGenerator.GenerateToken(user);
        var rawRefreshToken = refreshTokenGenerator.Generate();
        var refreshTokenHash = refreshTokenGenerator.ComputeHash(rawRefreshToken);
        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash);

        await writeRepository.AddRefreshTokenAsync(refreshToken, ct);
        await writeRepository.SaveChangesAsync(ct);

        var response = new AuthResponseDto(
            user.Id,
            jwtToken,
            rawRefreshToken);

        return Result<AuthResponseDto>.Ok(response);
    }
}