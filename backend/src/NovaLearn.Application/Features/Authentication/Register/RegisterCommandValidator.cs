using FluentValidation;
using NovaLearn.Application.Common.Validation;

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

        // Shared with the admin "add account" path via PasswordRules and mirrored on the client.
        // This validator is the authoritative gate: it runs before the identity store is touched.
        RuleFor(x => x.Password).MustBeAStrongPassword();

        RuleFor(x => x.AcceptedTerms)
            .Equal(true)
            .WithMessage("You must accept the Terms of Service and Privacy Policy to create an account.");
    }
}
