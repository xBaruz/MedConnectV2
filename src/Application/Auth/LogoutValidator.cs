using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Validation;

namespace MedConnect.Application.Auth;

public sealed class LogoutValidator : ICommandValidator<LogoutCommand>
{
    private static readonly RuleSet<LogoutCommand> Rules =
        new RuleSet<LogoutCommand>()
            .AddRule(command => !string.IsNullOrWhiteSpace(command.RefreshToken)&& command.RefreshToken.Length <= 200, new RuleViolation("InvalidRefreshToken", "Podaj refresh token, maksymalnie 200 znaków."));

    public IReadOnlyList<RuleViolation> Validate(LogoutCommand command)
    {
        return Rules.Validate(command);
    }
}