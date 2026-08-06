using FluentValidation;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.MarkEssayAnswer;

public sealed class MarkEssayAnswerCommandValidator : AbstractValidator<MarkEssayAnswerCommand>
{
    public MarkEssayAnswerCommandValidator()
    {
        RuleFor(x => x.AttemptId).NotEmpty();

        RuleFor(x => x.AnswerId).NotEmpty();

        // The per-question ceiling is enforced by the aggregate, which knows it. This only
        // rejects obvious nonsense before any work is done.
        RuleFor(x => x.PointsAwarded).InclusiveBetween(0, Question.MaxPointsCeiling);

        RuleFor(x => x.Feedback).MaximumLength(4000);
    }
}
