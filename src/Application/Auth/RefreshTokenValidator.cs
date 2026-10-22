using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Validation;

namespace MedConnect.Application.Auth;

public sealed class RefreshTokenValidator : ICommandValidator<RefreshTokenCommand>
{
    private static readonly RuleSet<RefreshTokenCommand> Rules =
        new RuleSet<RefreshTokenCommand>()
            .AddRule(command =>!string.IsNullOrWhiteSpace(command.RefreshToken) && command.RefreshToken.Length <= 200, new RuleViolation("InvalidRefreshToken", "Refresh token jest wymagany i może mieć maksymalnie 200 znaków."));

    public IReadOnlyList<RuleViolation> Validate(RefreshTokenCommand command)
    {
        return Rules.Validate(command);
    }
}