using FluentValidation;

namespace NovaLearn.Application.Features.Content.ReorderLessons;

public sealed class ReorderLessonsCommandValidator : AbstractValidator<ReorderLessonsCommand>
{
    public ReorderLessonsCommandValidator()
    {
        RuleFor(x => x.ModuleId)
            .NotEmpty();

        RuleFor(x => x.LessonIds)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.LessonIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("The order may not repeat a lesson.")
            .When(x => x.LessonIds is not null);
    }
}
