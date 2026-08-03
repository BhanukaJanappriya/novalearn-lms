using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.SaveAnswer;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

public sealed class SaveAnswerCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizzes = Substitute.For<IQuizRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly SaveAnswerCommandHandler _sut;

    private Quiz _quiz = null!;
    private Question _question = null!;

    public SaveAnswerCommandHandlerTests()
    {
        _currentUser.UserId.Returns(_studentId);
        _sut = new SaveAnswerCommandHandler(_quizzes, _unitOfWork, _currentUser);
    }

    private QuizAttempt Arrange(Guid? owner = null, bool submitted = false)
    {
        _quiz = Quiz.Create(
            Guid.NewGuid(), "Q", null, null, null, null, false, AssessmentStatus.Published);

        _question = _quiz.AddQuestion("Pick one", QuestionType.MultipleChoice, 10, 0, null);
        _question.ReplaceOptions([("Wrong", false), ("Right", true)]);

        QuizAttempt attempt = QuizAttempt.Start(_quiz.Id, owner ?? _studentId, 1, Now);
        if (submitted)
        {
            attempt.Submit(_quiz, Now.AddMinutes(1));
        }

        _quizzes.GetAttemptAsync(attempt.Id, Arg.Any<CancellationToken>()).Returns(attempt);
        _quizzes.GetQuizWithQuestionsAsync(_quiz.Id, Arg.Any<CancellationToken>()).Returns(_quiz);

        return attempt;
    }

    [Fact]
    public async Task A_learner_can_record_an_answer_on_their_own_open_attempt()
    {
        QuizAttempt attempt = Arrange();

        Result result = await _sut.Handle(
            new SaveAnswerCommand(attempt.Id, _question.Id, _question.Options.First().Id, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        attempt.Answers.Should().ContainSingle();
        await _quizzes.Received(1).AddAnswerAsync(Arg.Any<AttemptAnswer>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>Answering on someone else's attempt would fabricate their result.</summary>
    [Fact]
    public async Task A_learner_cannot_answer_on_someone_elses_attempt()
    {
        QuizAttempt attempt = Arrange(owner: Guid.NewGuid());

        Result result = await _sut.Handle(
            new SaveAnswerCommand(attempt.Id, _question.Id, null, "x"), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.NotAttemptOwner);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_submitted_attempt_refuses_further_answers()
    {
        QuizAttempt attempt = Arrange(submitted: true);

        Result result = await _sut.Handle(
            new SaveAnswerCommand(attempt.Id, _question.Id, null, "x"), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.AttemptAlreadySubmitted);
    }

    /// <summary>Guards against an answer being planted against a question from another quiz.</summary>
    [Fact]
    public async Task An_answer_to_a_question_outside_the_quiz_is_rejected()
    {
        QuizAttempt attempt = Arrange();

        Result result = await _sut.Handle(
            new SaveAnswerCommand(attempt.Id, Guid.NewGuid(), null, "x"), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.QuestionNotInAttempt);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A second answer to the same question must update the existing row, not insert another,
    /// which the unique index would reject anyway.
    /// </summary>
    [Fact]
    public async Task Answering_the_same_question_again_updates_rather_than_inserts()
    {
        QuizAttempt attempt = Arrange();
        await _sut.Handle(
            new SaveAnswerCommand(attempt.Id, _question.Id, _question.Options.First().Id, null),
            CancellationToken.None);

        _quizzes.ClearReceivedCalls();

        Result result = await _sut.Handle(
            new SaveAnswerCommand(attempt.Id, _question.Id, _question.Options.Last().Id, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        attempt.Answers.Should().ContainSingle();
        await _quizzes.DidNotReceive().AddAnswerAsync(Arg.Any<AttemptAnswer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_missing_attempt_is_reported_as_not_found()
    {
        _quizzes.GetAttemptAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((QuizAttempt?)null);

        Result result = await _sut.Handle(
            new SaveAnswerCommand(Guid.NewGuid(), Guid.NewGuid(), null, "x"), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.AttemptNotFound);
    }
}
