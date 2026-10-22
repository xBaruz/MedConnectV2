using System.Net.Mail;
using MedConnect.Core.Abstractions;
using MedConnect.Core.Commands;
using MedConnect.Validation;

namespace MedConnect.Application.Auth;

public sealed class RegisterUserValidator
    : ICommandValidator<RegisterUserCommand>
{
    private static readonly RuleSet<RegisterUserCommand> Rules =
        new RuleSet<RegisterUserCommand>()
            .AddRule(command => IsValidEmail(command.Email),new RuleViolation("InvalidEmail", "Podaj poprawny adres email, maksymalnie 256 znaków."))
            .AddRule(command => !string.IsNullOrWhiteSpace(command.Password) && command.Password.Length >= 5, new RuleViolation("InvalidPassword", "Hasło musi mieć co najmniej 5 znaków."))
            .AddRule(command => !string.IsNullOrWhiteSpace(command.Firstname) && command.Firstname.Trim().Length <= 100, new RuleViolation("InvalidFirstname", "Imię jest wymagane i może mieć maksymalnie 100 znaków."))
            .AddRule(command => !string.IsNullOrWhiteSpace(command.Lastname) && command.Lastname.Trim().Length <= 100, new RuleViolation("InvalidLastname", "Nazwisko jest wymagane i może mieć maksymalnie 100 znaków."))
            .AddRule(command => command.DateBirth != default && command.DateBirth < DateOnly.FromDateTime(DateTime.UtcNow), new RuleViolation( "InvalidBirthDate", "Podaj datę urodzenia z przeszłości."));

    public IReadOnlyList<RuleViolation> Validate(RegisterUserCommand command)
    {
        return Rules.Validate(command);
    }

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var trimmedEmail = email.Trim();

        return trimmedEmail.Length <= 256 && MailAddress.TryCreate(trimmedEmail, out var address) && address.Address == trimmedEmail;
    }
}