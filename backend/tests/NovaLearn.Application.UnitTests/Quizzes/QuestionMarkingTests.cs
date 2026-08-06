using FluentAssertions;
using NovaLearn.Domain.Quizzes;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

/// <summary>
/// Marking lives on the question, so these cover the actual scoring rules rather than the
/// plumbing around them.
/// </summary>
public sealed class QuestionMarkingTests
{
    private static Question MultipleChoice(int points = 10)
    {
        Question question = Question.Create(Guid.NewGuid(), "  Pick one  ", QuestionType.MultipleChoice, points, 0);
        question.ReplaceOptions([("Wrong", false), ("Right", true), ("Also wrong", false)]);
        return question;
    }

    private static Question ShortAnswer(int points = 5, params string[] accepted) =>
        Question.Create(Guid.NewGuid(), "Name it", QuestionType.ShortAnswer, points, 0, accepted);

    [Fact]
    public void Text_is_trimmed_and_points_are_clamped()
    {
        Question question = Question.Create(Guid.NewGuid(), "  Q  ", QuestionType.TrueFalse, 9999, -5);

        question.Text.Should().Be("Q");
        question.Points.Should().Be(Question.MaxPointsCeiling);
        question.SortOrder.Should().Be(0);
    }

    [Fact]
    public void The_correct_option_scores_full_points()
    {
        Question question = MultipleChoice(points: 10);
        Guid correct = question.Options.Single(o => o.IsCorrect).Id;

        question.Mark([correct], null).Should().Be(10);
    }

    [Fact]
    public void A_wrong_option_scores_nothing()
    {
        Question question = MultipleChoice();
        Guid wrong = question.Options.First(o => !o.IsCorrect).Id;

        question.Mark([wrong], null).Should().Be(0);
    }

    [Fact]
    public void An_unanswered_option_question_scores_nothing()
    {
        MultipleChoice().Mark([], null).Should().Be(0);
    }

    /// <summary>Guards against an option id from a different question being passed in.</summary>
    [Fact]
    public void An_option_that_belongs_to_another_question_scores_nothing()
    {
        Question question = MultipleChoice();
        Question other = MultipleChoice();
        Guid otherCorrect = other.Options.Single(o => o.IsCorrect).Id;

        question.Mark([otherCorrect], null).Should().Be(0);
    }

    [Theory]
    [InlineData("Paris", 5)]
    [InlineData("paris", 5)]
    [InlineData("  PARIS  ", 5)]
    [InlineData("Lyon", 0)]
    [InlineData("", 0)]
    [InlineData(null, 0)]
    public void Short_answers_match_case_insensitively_and_ignore_surrounding_space(string? given, int expected)
    {
        ShortAnswer(5, "Paris").Mark([], given).Should().Be(expected);
    }

    [Fact]
    public void Any_of_several_accepted_answers_is_correct()
    {
        Question question = ShortAnswer(5, "Paris", "City of Light");

        question.Mark([], "city of light").Should().Be(5);
        question.Mark([], "Paris").Should().Be(5);
    }

    [Fact]
    public void Blank_accepted_answers_are_discarded()
    {
        Question question = ShortAnswer(5, "  ", "", "Paris");

        question.AcceptedAnswerList.Should().ContainSingle().Which.Should().Be("Paris");
    }

    [Fact]
    public void An_option_question_needs_two_options_and_exactly_one_correct()
    {
        Question question = Question.Create(Guid.NewGuid(), "Q", QuestionType.MultipleChoice, 5, 0);
        question.IsAnswerable().Should().BeFalse("it has no options");

        question.ReplaceOptions([("Only", true)]);
        question.IsAnswerable().Should().BeFalse("one option is not a choice");

        question.ReplaceOptions([("A", true), ("B", true)]);
        question.IsAnswerable().Should().BeFalse("two correct answers cannot be marked");

        question.ReplaceOptions([("A", true), ("B", false)]);
        question.IsAnswerable().Should().BeTrue();
    }

    [Fact]
    public void A_short_answer_question_needs_an_accepted_answer()
    {
        Question.Create(Guid.NewGuid(), "Q", QuestionType.ShortAnswer, 5, 0)
            .IsAnswerable().Should().BeFalse();

        ShortAnswer(5, "Paris").IsAnswerable().Should().BeTrue();
    }

    /// <summary>
    /// Options left behind on a question switched to short answer would be dead rows that the
    /// author cannot see or remove.
    /// </summary>
    [Fact]
    public void Switching_to_a_short_answer_question_drops_its_options()
    {
        Question question = MultipleChoice();
        question.Options.Should().NotBeEmpty();

        question.Update("Now typed", QuestionType.ShortAnswer, 5, ["Paris"]);

        question.Options.Should().BeEmpty();
        question.IsAnswerable().Should().BeTrue();
    }
}
