using FluentValidation;

namespace NovaLearn.Application.Features.Content.ReorderModules;

public sealed class ReorderModulesCommandValidator : AbstractValidator<ReorderModulesCommand>
{
    public ReorderModulesCommandValidator()
    {
        RuleFor(x => x.CourseId)
            .NotEmpty();

        RuleFor(x => x.ModuleIds)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.ModuleIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("The order may not repeat a module.")
            .When(x => x.ModuleIds is not null);
    }
}
