using FluentValidation;

namespace NovaLearn.Application.Features.Authentication.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        // Kept in step with the client-side rules in frontend/src/features/auth/schemas.ts and the
        // Identity options in NovaLearn.Persistence. This validator is the authoritative gate: it
        // runs before the identity store is touched and can express the two-digit rule that
        // Identity's PasswordOptions cannot.
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(9).WithMessage("Password must be longer than 8 characters.")
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character (for example / _ - @).")
            .Must(HasAtLeastTwoDigits).WithMessage("Password must contain at least two numbers.");

        RuleFor(x => x.AcceptedTerms)
            .Equal(true)
            .WithMessage("You must accept the Terms of Service and Privacy Policy to create an account.");
    }

    private static bool HasAtLeastTwoDigits(string? password) =>
        password is not null && password.Count(char.IsDigit) >= 2;
}
