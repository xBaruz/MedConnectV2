using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Core.Dtos;
using MedConnect.Infrastructure.Security;
using MedConnect.Models;
using MedConnect.Repositories;

namespace MedConnect.Application.Auth;

public sealed class RefreshTokenHandler(
    IUserReadRepository readRepository,
    IUserWriteRepository writeRepository,
    IJwtTokenGenerator tokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator)
    : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<Result<AuthResponseDto>> HandleAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var tokenHash = refreshTokenGenerator.ComputeHash(command.RefreshToken);

        var existingToken = await readRepository.GetRefreshTokenByHashAsync(tokenHash, ct);

        if (existingToken is null || !existingToken.IsActive)
            return InvalidToken();

        var user = await readRepository.GetByIdAsync(existingToken.UserId, ct);

        if (user is null)
            return InvalidToken();

        var rawNewRefreshToken = refreshTokenGenerator.Generate();
        var newTokenHash = refreshTokenGenerator.ComputeHash(rawNewRefreshToken);
        var newRefreshToken = RefreshToken.Create(user.Id, newTokenHash);
        var jwtToken = tokenGenerator.GenerateToken(user);

        existingToken.Revoke();

        await writeRepository.AddRefreshTokenAsync(newRefreshToken, ct);

        var saved = await writeRepository.TrySaveRefreshTokenChangesAsync(ct);

        if (!saved)
        {
            return InvalidToken();
        }

        var response = new AuthResponseDto(
            user.Id,
            jwtToken,
            rawNewRefreshToken);

        return Result<AuthResponseDto>.Ok(response);
    }

    private static Result<AuthResponseDto> InvalidToken()
    {
        return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Nieprawidłowy, wygasły lub wykorzystany refresh token."));
    }
}