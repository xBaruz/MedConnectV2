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
    IJwtTokenGenerator tokenGenerator)
    : ICommandHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<Result<AuthResponseDto>> HandleAsync(RefreshTokenCommand command, CancellationToken ct = default)
    {
        var existingToken = await readRepository.GetRefreshTokenAsync(command.RefreshToken, ct);

        if (existingToken is null || !existingToken.IsActive)
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Nieprawidłowy lub wygasły refresh token."));

        var user = await readRepository.GetByIdAsync(existingToken.UserId, ct);

        if (user is null)
            return Result<AuthResponseDto>.Fail(new Error.Unauthorized("Użytkownik nie istnieje."));
        

        existingToken.Revoke();

        var jwtToken = tokenGenerator.GenerateToken(user);
        var newRefreshToken = RefreshToken.Create(user.Id);

        await writeRepository.AddRefreshTokenAsync(newRefreshToken,ct);

        await writeRepository.SaveChangesAsync(ct);

        var response = new AuthResponseDto(
            user.Id,
            jwtToken,
            newRefreshToken.Token);

        return Result<AuthResponseDto>.Ok(response);
    }
}