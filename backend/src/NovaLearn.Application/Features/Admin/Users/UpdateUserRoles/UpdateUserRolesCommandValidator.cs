using FluentValidation;

namespace NovaLearn.Application.Features.Admin.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandValidator : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        // An account with no role can sign in but reach nothing, which looks like a broken
        // account rather than a deliberate state. Deactivate instead.
        RuleFor(x => x.Roles)
            .NotNull()
            .Must(roles => roles.Count > 0)
            .WithMessage("Assign at least one role. To revoke access, deactivate the account instead.");

        RuleFor(x => x.Roles)
            .Must(roles => roles.Distinct(StringComparer.Ordinal).Count() == roles.Count)
            .WithMessage("Roles must not contain duplicates.")
            .When(x => x.Roles is not null);
    }
}
