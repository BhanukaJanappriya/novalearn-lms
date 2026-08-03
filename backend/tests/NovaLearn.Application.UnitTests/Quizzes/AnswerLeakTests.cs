using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Domain.Quizzes;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

/// <summary>
/// The security boundary of the whole quiz feature: what a learner receives while sitting a quiz
/// must not contain the answer key. These assert on the serialised payload, because that is what
/// actually reaches the browser, and structurally on the type, so a future field cannot quietly
/// reintroduce the leak.
/// </summary>
public sealed class AnswerLeakTests
{
    private static Question BuildQuestion()
    {
        Question question = Question.Create(
            Guid.NewGuid(), "Which is right?", QuestionType.MultipleChoice, 10, 0);

        question.ReplaceOptions([("Decoy", false), ("The answer", true)]);
        return question;
    }

    /// <summary>Matches how the API serialises, so the assertions are about the real payload.</summary>
    private static readonly JsonSerializerOptions ApiOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private static string SerialiseForLearner(Question question) =>
        JsonSerializer.Serialize(TakingQuestionDto.FromEntity(question, null), ApiOptions);

    [Fact]
    public void The_taking_payload_does_not_mention_correctness()
    {
        string json = SerialiseForLearner(BuildQuestion());

        json.Should().NotContainEquivalentOf(
            "isCorrect", "the learner payload must not say which option is right");
        json.Should().NotContainEquivalentOf("acceptedAnswers");
        json.Should().NotContainEquivalentOf("correct");
    }

    [Fact]
    public void The_taking_payload_still_carries_the_option_text_learners_need()
    {
        string json = SerialiseForLearner(BuildQuestion());

        json.Should().Contain("Decoy");
        json.Should().Contain("The answer", "options must be selectable, just not labelled");
    }

    [Fact]
    public void A_short_answer_question_does_not_ship_its_accepted_answers()
    {
        Question question = Question.Create(
            Guid.NewGuid(), "Capital of France", QuestionType.ShortAnswer, 5, 0, ["Paris", "City of Light"]);

        string json = SerialiseForLearner(question);

        json.Should().NotContain("Paris");
        json.Should().NotContain("City of Light");
    }

    /// <summary>
    /// Structural guard. Adding a bool called something like <c>IsCorrect</c> to the taking
    /// option shape would leak every answer, and would be easy to do by copying the authoring
    /// type. This fails the moment that happens.
    /// </summary>
    [Fact]
    public void The_taking_option_shape_exposes_nothing_beyond_id_and_text()
    {
        string[] properties = typeof(TakingOptionDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        properties.Should().BeEquivalentTo(["Id", "Text"]);
    }

    [Fact]
    public void The_taking_question_shape_carries_no_answer_bearing_member()
    {
        string[] properties = typeof(TakingQuestionDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => p.Name)
            .ToArray();

        properties.Should().NotContain("AcceptedAnswers");
        properties.Should().NotContain("AcceptedAnswerList");
        properties.Should().NotContain("IsCorrect");
        properties.Should().NotContain("CorrectOptionId");
    }

    /// <summary>The authoring shape is the one allowed to carry answers; staff only ever see it.</summary>
    [Fact]
    public void The_authoring_payload_does_carry_the_answer_key()
    {
        string json = JsonSerializer.Serialize(AuthoringQuestionDto.FromEntity(BuildQuestion()), ApiOptions);

        json.Should().ContainEquivalentOf("isCorrect");
    }
}
