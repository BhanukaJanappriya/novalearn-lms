using FluentValidation;
using NovaLearn.Domain.Content;

namespace NovaLearn.Application.Features.Content.UpdateLesson;

public sealed class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DurationMinutes.HasValue);

        // A text lesson carries its body inline; every other type points at a resource.
        RuleFor(x => x.TextContent)
            .NotEmpty()
            .MaximumLength(20000)
            .When(x => x.Type == LessonType.Text);

        RuleFor(x => x.ContentUrl)
            .NotEmpty()
            .MaximumLength(1024)
            .When(x => x.Type != LessonType.Text);
    }
}
