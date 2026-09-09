using FluentValidation;
using NovaLearn.Application.Common.Validation;
using NovaLearn.Domain.Identity;

namespace NovaLearn.Application.Features.Admin.Users.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
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

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => Roles.All.Contains(role))
            .WithMessage(x => $"'{x.Role}' is not a known role.");

        RuleFor(x => x.Password).MustBeAStrongPassword();
    }
}
