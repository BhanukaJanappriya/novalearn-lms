using FluentValidation;
using NovaLearn.Domain.Quizzes;

namespace NovaLearn.Application.Features.Quizzes.SaveQuestion;

public sealed class SaveQuestionCommandValidator : AbstractValidator<SaveQuestionCommand>
{
    /// <summary>The types answered by picking from a list.</summary>
    private static bool IsOptionBased(QuestionType type) =>
        type is QuestionType.MultipleChoice or QuestionType.TrueFalse or QuestionType.MultipleResponse;

    public SaveQuestionCommandValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();

        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Points).InclusiveBetween(1, Question.MaxPointsCeiling);

        RuleFor(x => x.MarkingGuidance).MaximumLength(2000);

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

        // An option question needs a real choice to make.
        RuleFor(x => x.Options)
            .Must(options => options.Count >= 2)
            .WithMessage("Add at least two options.")
            .When(x => IsOptionBased(x.Type));

        // Single-answer types must have exactly one key, or scoring is ambiguous.
        RuleFor(x => x.Options)
            .Must(options => options.Count(o => o.IsCorrect) == 1)
            .WithMessage("Mark exactly one option as correct.")
            .When(x => x.Type is QuestionType.MultipleChoice or QuestionType.TrueFalse);

        // Checkboxes allow several correct answers, but at least one is still needed.
        RuleFor(x => x.Options)
            .Must(options => options.Any(o => o.IsCorrect))
            .WithMessage("Mark at least one option as correct.")
            .When(x => x.Type == QuestionType.MultipleResponse);

        // An essay has no key by design, so options here would be dead data.
        RuleFor(x => x.Options)
            .Must(options => options.Count == 0)
            .WithMessage("An essay question does not take options.")
            .When(x => x.Type == QuestionType.Essay);
    }
}
