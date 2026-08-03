using FluentValidation;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.CreateQuiz;

public sealed class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(4000);

        // The aggregate clamps too, but rejecting out-of-range input up front gives the client a
        // 400 with a field name instead of a silently adjusted value.
        RuleFor(x => x.TimeLimitMinutes)
            .InclusiveBetween(1, Quiz.MaxTimeLimitMinutes)
            .When(x => x.TimeLimitMinutes.HasValue);

        RuleFor(x => x.MaxAttempts)
            .GreaterThanOrEqualTo(1)
            .When(x => x.MaxAttempts.HasValue);

        RuleFor(x => x.PassingScorePercent)
            .InclusiveBetween(0, 100)
            .When(x => x.PassingScorePercent.HasValue);

        RuleFor(x => x.Status).IsInEnum();
    }
}
