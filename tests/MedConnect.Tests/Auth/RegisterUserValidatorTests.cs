using MedConnect.Application.Auth;
using MedConnect.Core.Commands;
using Xunit;

namespace MedConnect.Tests.Auth;

public sealed class RegisterUserValidatorTests
{
    private readonly RegisterUserValidator _validator = new();

    [Fact]
    public void Validate_WhenDataIsValid_ReturnsNoErrors()
    {
        // Arrange: przygotowanie danych.
        var command = ValidCommand();

        // Act: wykonanie sprawdzanej operacji.
        var errors = _validator.Validate(command);

        // Assert: sprawdzenie oczekiwanego wyniku.
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("     ", false)]
    [InlineData("abcd", false)]
    [InlineData("abcde", true)]
    [InlineData("dluzsze-haslo", true)]
    public void Validate_ChecksConfiguredPasswordMinimum(string password, bool shouldBeAccepted)
    {
        var command = ValidCommand() with
        {
            Password = password
        };

        var errors = _validator.Validate(command);

        var hasPasswordError = errors.Any(error => error.Code == "InvalidPassword");

        Assert.Equal(!shouldBeAccepted, hasPasswordError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("niepoprawny-email")]
    public void Validate_WhenEmailIsInvalid_ReturnsEmailError(
        string email)
    {
        var command = ValidCommand() with
        {
            Email = email
        };

        var errors = _validator.Validate(command);

        Assert.Contains(
            errors,
            error => error.Code == "InvalidEmail");
    }

    [Fact]
    public void Validate_WhenFirstNameIsTooLong_ReturnsError()
    {
        var command = ValidCommand() with
        {
            Firstname = new string('A', 101)
        };

        var errors = _validator.Validate(command);

        Assert.Contains(
            errors,
            error => error.Code == "InvalidFirstname");
    }

    [Fact]
    public void Validate_WhenBirthDateIsMissing_ReturnsError()
    {
        var command = ValidCommand() with
        {
            DateBirth = default
        };

        var errors = _validator.Validate(command);

        Assert.Contains(
            errors,
            error => error.Code == "InvalidBirthDate");
    }

    [Fact]
    public void Validate_WhenBirthDateIsInFuture_ReturnsError()
    {
        var command = ValidCommand() with
        {
            DateBirth = DateOnly
                .FromDateTime(DateTime.UtcNow)
                .AddYears(1)
        };

        var errors = _validator.Validate(command);

        Assert.Contains(
            errors,
            error => error.Code == "InvalidBirthDate");
    }

    private static RegisterUserCommand ValidCommand()
    {
        return new RegisterUserCommand(
            Email: "jan@example.test",
            Password: "abcde",
            Firstname: "Jan",
            Lastname: "Kowalski",
            DateBirth: new DateOnly(1995, 5, 12));
    }
}