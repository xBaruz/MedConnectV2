using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Validation;

namespace MedConnect.Application.Auth;

public sealed class RefreshTokenValidator : ICommandValidator<RefreshTokenCommand>
{
    private static readonly RuleSet<RefreshTokenCommand> Rules =
        new RuleSet<RefreshTokenCommand>()
            .AddRule(command => !string.IsNullOrWhiteSpace(command.RefreshToken), new RuleViolation("RefreshTokenRequired", "Refresh token jest wymagany."));

    public IReadOnlyList<RuleViolation> Validate(RefreshTokenCommand command)
    {
        return Rules.Validate(command);
    }
}