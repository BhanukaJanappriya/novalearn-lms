using FluentValidation.TestHelper;
using NovaLearn.Application.Features.Authentication.Register;
using Xunit;

namespace NovaLearn.Application.UnitTests.Authentication;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    private static RegisterCommand ValidCommand() =>
        new("Ada", "Lovelace", "ada@novalearn.local", "Str0ng-Pass12", AcceptedTerms: true);

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
    [InlineData("Ab12!xyz")]         // 8 characters, one short of the minimum
    [InlineData("alllowercase12!")]  // no uppercase letter
    [InlineData("ALLUPPERCASE12!")]  // no lowercase letter
    [InlineData("NoNumbersHere!!x")] // no digits
    [InlineData("OnlyOne1Digit!")]   // only one digit, needs two
    [InlineData("NoSpecialChar123")] // no special character
    public void Weak_password_fails(string password)
    {
        RegisterCommand command = ValidCommand() with { Password = password };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_at_the_nine_character_minimum_with_two_digits_passes()
    {
        RegisterCommand command = ValidCommand() with { Password = "Abc12!def" };
        _validator.TestValidate(command).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Not_accepting_the_terms_fails()
    {
        RegisterCommand command = ValidCommand() with { AcceptedTerms = false };
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.AcceptedTerms);
    }
}
