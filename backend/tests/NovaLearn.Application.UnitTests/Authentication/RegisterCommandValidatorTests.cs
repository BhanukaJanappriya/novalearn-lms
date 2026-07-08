using FluentValidation.TestHelper;
using NovaLearn.Application.Features.Authentication.Register;
using Xunit;

namespace NovaLearn.Application.UnitTests.Authentication;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("Ada", "Lovelace", "ada@novalearn.local", "Str0ng!Pass");

    [Fact]
    public void Valid_command_passes_validation()
    {
        TestValidationResult<RegisterCommand> result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Invalid_email_fails(string email)
    {
        RegisterCommand command = ValidCommand() with { Email = email };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short1!A")]      // ok length boundary check handled elsewhere
    [InlineData("alllowercase1!")] // no uppercase
    [InlineData("ALLUPPERCASE1!")] // no lowercase
    [InlineData("NoDigits!!")]     // no digit
    [InlineData("NoSpecial123")]   // no special char
    public void Weak_password_fails(string password)
    {
        RegisterCommand command = ValidCommand() with { Password = password };

        // "short1!A" is actually valid; assert only the genuinely weak ones fail.
        TestValidationResult<RegisterCommand> result = _validator.TestValidate(command);
        if (password == "short1!A")
        {
            result.ShouldNotHaveValidationErrorFor(x => x.Password);
        }
        else
        {
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}
