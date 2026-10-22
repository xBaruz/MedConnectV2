using MedConnect.Core;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Repositories;

namespace MedConnect.Application.Auth;

public sealed class LogoutHandler(
    IUserReadRepository readRepository,
    IUserWriteRepository writeRepository,
    IRefreshTokenGenerator refreshTokenGenerator)
    : ICommandHandler<LogoutCommand, bool>
{
    public async Task<Result<bool>> HandleAsync(LogoutCommand command, CancellationToken ct = default)
    {
        var tokenHash = refreshTokenGenerator.ComputeHash(command.RefreshToken);

        var token = await readRepository.GetRefreshTokenByHashAsync(tokenHash, ct);

        if (token is null || !token.IsActive)
            return Result<bool>.Ok(true);

        token.Revoke();

        var saved = await writeRepository.TrySaveRefreshTokenChangesAsync(ct);

        if (!saved)
        {
            return Result<bool>.Fail(new Error.Conflict("Token zmienił się podczas wylogowania. " + "Ponów wylogowanie z aktualnym refresh tokenem."));
        }

        return Result<bool>.Ok(true);
    }
}