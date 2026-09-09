using FluentAssertions;
using FluentValidation.TestHelper;
using NovaLearn.Application.Features.Admin.Users.CreateUser;
using NovaLearn.Domain.Identity;
using Xunit;

namespace NovaLearn.Application.UnitTests.Admin;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    private static CreateUserCommand ValidCommand() =>
        new("Grace", "Hopper", "grace.hopper@novalearn.local", Roles.Student, "Str0ng-Pass12");

    [Fact]
    public void A_well_formed_command_passes()
    {
        _validator.TestValidate(ValidCommand()).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("Manager")]
    [InlineData("")]
    [InlineData("student")]
    public void An_unknown_role_fails(string role)
    {
        _validator.TestValidate(ValidCommand() with { Role = role })
            .ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("weakpass")]      // too short, no upper, one digit short, no symbol
    [InlineData("OneDigit1!x")]   // only one digit
    public void A_weak_password_fails(string password)
    {
        _validator.TestValidate(ValidCommand() with { Password = password })
            .ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("", "Hopper", "grace@novalearn.local")]
    [InlineData("Grace", "", "grace@novalearn.local")]
    [InlineData("Grace", "Hopper", "not-an-email")]
    public void Missing_or_malformed_identity_fields_fail(string first, string last, string email)
    {
        CreateUserCommand command = ValidCommand() with
        {
            FirstName = first,
            LastName = last,
            Email = email,
        };

        _validator.Validate(command).IsValid.Should().BeFalse();
    }
}
