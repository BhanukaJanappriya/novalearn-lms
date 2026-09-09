using FluentValidation;

namespace NovaLearn.Application.Common.Validation;

/// <summary>
/// The one place the account password policy is expressed on the server. Applied wherever a
/// caller sets a password: self-registration and an administrator adding an account. Mirrored on
/// the client in <c>frontend/src/lib/passwordPolicy.ts</c> for live feedback; this rule is the
/// authoritative gate.
/// </summary>
public static class PasswordRules
{
    public const int MinimumLength = 9;
    public const int MaximumLength = 128;

    /// <summary>
    /// Requires more than eight characters, at least one uppercase and one lowercase letter,
    /// at least two digits, and at least one non-alphanumeric character.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeAStrongPassword<T>(
        this IRuleBuilder<T, string> rule) =>
        rule
            .NotEmpty()
            .MinimumLength(MinimumLength).WithMessage("Password must be longer than 8 characters.")
            .MaximumLength(MaximumLength)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character (for example / _ - @).")
            .Must(HasAtLeastTwoDigits).WithMessage("Password must contain at least two numbers.");

    private static bool HasAtLeastTwoDigits(string? password) =>
        password is not null && password.Count(char.IsDigit) >= 2;
}
