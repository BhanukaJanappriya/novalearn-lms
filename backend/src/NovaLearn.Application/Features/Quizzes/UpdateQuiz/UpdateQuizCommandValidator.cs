using FluentValidation;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.UpdateQuiz;

public sealed class UpdateQuizCommandValidator : AbstractValidator<UpdateQuizCommand>
{
    public UpdateQuizCommandValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(4000);

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
