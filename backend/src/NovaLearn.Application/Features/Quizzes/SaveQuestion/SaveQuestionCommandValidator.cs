using FluentValidation;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.SaveQuestion;

public sealed class SaveQuestionCommandValidator : AbstractValidator<SaveQuestionCommand>
{
    public SaveQuestionCommandValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();

        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Points).InclusiveBetween(1, Question.MaxPointsCeiling);

        RuleForEach(x => x.Options)
            .ChildRules(option => option.RuleFor(o => o.Text).NotEmpty().MaximumLength(1000));

        RuleForEach(x => x.AcceptedAnswers)
            .NotEmpty()
            .MaximumLength(500);

        // A short-answer question is marked by comparing text, so it needs something to compare to.
        RuleFor(x => x.AcceptedAnswers)
            .Must(answers => answers.Any(a => !string.IsNullOrWhiteSpace(a)))
            .WithMessage("Add at least one accepted answer.")
            .When(x => x.Type == QuestionType.ShortAnswer);

        // An option question needs a real choice, and exactly one right answer to mark against.
        RuleFor(x => x.Options)
            .Must(options => options.Count >= 2)
            .WithMessage("Add at least two options.")
            .When(x => x.Type != QuestionType.ShortAnswer);

        RuleFor(x => x.Options)
            .Must(options => options.Count(o => o.IsCorrect) == 1)
            .WithMessage("Mark exactly one option as correct.")
            .When(x => x.Type != QuestionType.ShortAnswer);
    }
}
