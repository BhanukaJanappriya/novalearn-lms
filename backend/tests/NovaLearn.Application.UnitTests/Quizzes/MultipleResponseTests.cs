using FluentAssertions;
using NovaLearn.Domain.Quizzes;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

/// <summary>
/// Checkbox questions, marked all or nothing. Half-right scoring nothing is the deliberate
/// choice: partial credit on a set needs a rubric the author never supplied.
/// </summary>
public sealed class MultipleResponseTests
{
    private static Question Build(int points = 10)
    {
        Question question = Question.Create(
            Guid.NewGuid(), "Pick every prime", QuestionType.MultipleResponse, points, 0);

        question.ReplaceOptions([("2", true), ("3", true), ("4", false), ("9", false)]);
        return question;
    }

    private static Guid[] CorrectOf(Question q) => q.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToArray();

    private static Guid[] WrongOf(Question q) => q.Options.Where(o => !o.IsCorrect).Select(o => o.Id).ToArray();

    [Fact]
    public void Selecting_exactly_the_correct_set_scores_full_points()
    {
        Question question = Build(10);

        question.Mark(CorrectOf(question), null).Should().Be(10);
    }

    [Fact]
    public void Order_of_selection_does_not_matter()
    {
        Question question = Build(10);

        question.Mark(CorrectOf(question).Reverse().ToArray(), null).Should().Be(10);
    }

    [Fact]
    public void Missing_one_correct_option_scores_nothing()
    {
        Question question = Build();

        question.Mark([CorrectOf(question).First()], null).Should().Be(0);
    }

    [Fact]
    public void Adding_a_wrong_option_to_a_correct_set_scores_nothing()
    {
        Question question = Build();
        Guid[] correctPlusOne = [.. CorrectOf(question), WrongOf(question).First()];

        question.Mark(correctPlusOne, null).Should().Be(0);
    }

    [Fact]
    public void Selecting_nothing_scores_nothing()
    {
        Build().Mark([], null).Should().Be(0);
    }

    [Fact]
    public void An_option_from_another_question_cannot_stand_in_for_a_correct_one()
    {
        Question question = Build();
        Question other = Build();

        question.Mark([CorrectOf(question).First(), CorrectOf(other).Last()], null).Should().Be(0);
    }

    [Fact]
    public void A_checkbox_question_only_needs_one_correct_option_to_be_answerable()
    {
        Question question = Question.Create(
            Guid.NewGuid(), "Pick", QuestionType.MultipleResponse, 5, 0);

        question.ReplaceOptions([("A", true), ("B", false)]);
        question.IsAnswerable().Should().BeTrue();

        // Unlike single choice, several correct options are valid here.
        question.ReplaceOptions([("A", true), ("B", true)]);
        question.IsAnswerable().Should().BeTrue();

        question.ReplaceOptions([("A", false), ("B", false)]);
        question.IsAnswerable().Should().BeFalse("nothing to mark against");
    }

    [Fact]
    public void Only_a_checkbox_question_allows_several_selections()
    {
        Build().AllowsMultipleSelections.Should().BeTrue();

        Question.Create(Guid.NewGuid(), "Q", QuestionType.MultipleChoice, 5, 0)
            .AllowsMultipleSelections.Should().BeFalse();
    }
}
