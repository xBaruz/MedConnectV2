using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Validation;

namespace MedConnect.Application.Auth;

public sealed class LoginValidator : ICommandValidator<LoginCommand>
{
    private static readonly RuleSet<LoginCommand> Rules =
        new RuleSet<LoginCommand>()
            .AddRule(command => !string.IsNullOrWhiteSpace(command.Email), new RuleViolation("EmailRequired", "Email jest wymagany."))
            .AddRule(command => !string.IsNullOrWhiteSpace(command.Password), new RuleViolation("PasswordRequired", "Hasło jest wymagane."));

    public IReadOnlyList<RuleViolation> Validate(LoginCommand command)
    {
        return Rules.Validate(command);
    }
}