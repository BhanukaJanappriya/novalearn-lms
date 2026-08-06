using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Application.Features.Quizzes.StartAttempt;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Domain.Quizzes;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Quizzes;

public sealed class StartAttemptCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 3, 10, 0, 0, TimeSpan.Zero);

    private readonly IQuizRepository _quizzes = Substitute.For<IQuizRepository>();
    private readonly IEnrollmentRepository _enrollments = Substitute.For<IEnrollmentRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly Guid _courseId = Guid.NewGuid();
    private readonly StartAttemptCommandHandler _sut;

    public StartAttemptCommandHandlerTests()
    {
        _dateTime.UtcNow.Returns(Now);
        _currentUser.UserId.Returns(_studentId);
        _sut = new StartAttemptCommandHandler(
            _quizzes, _enrollments, _unitOfWork, _currentUser, _dateTime);
    }

    private Quiz Arrange(
        AssessmentStatus status = AssessmentStatus.Published,
        int? maxAttempts = null,
        bool enrolled = true,
        int submittedAttempts = 0,
        QuizAttempt? openAttempt = null)
    {
        Quiz quiz = Quiz.Create(
            _courseId, "Week 1 check", null, 30, maxAttempts, 50, false, status);

        Question question = quiz.AddQuestion("Pick one", QuestionType.MultipleChoice, 10, 0, null);
        question.ReplaceOptions([("Wrong", false), ("Right", true)]);

        _quizzes.GetQuizWithQuestionsAsync(quiz.Id, Arg.Any<CancellationToken>()).Returns(quiz);

        _enrollments.GetActiveAsync(_studentId, _courseId, Arg.Any<CancellationToken>())
            .Returns(enrolled ? Enrollment.Create(_studentId, _courseId, Now.AddDays(-5)) : null);

        _quizzes.GetOpenAttemptAsync(quiz.Id, _studentId, Arg.Any<CancellationToken>())
            .Returns(openAttempt);

        var previous = new List<QuizAttempt>();
        for (int i = 0; i < submittedAttempts; i++)
        {
            QuizAttempt done = QuizAttempt.Start(quiz.Id, _studentId, i + 1, Now.AddDays(-1));
            done.Submit(quiz, Now.AddDays(-1).AddMinutes(5));
            previous.Add(done);
        }

        _quizzes.ListAttemptsForStudentAsync(quiz.Id, _studentId, Arg.Any<CancellationToken>())
            .Returns(previous);

        return quiz;
    }

    [Fact]
    public async Task An_enrolled_learner_can_start_a_published_quiz()
    {
        Quiz quiz = Arrange();

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AttemptNumber.Should().Be(1);
        result.Value.DeadlineUtc.Should().Be(Now.AddMinutes(30));
        await _quizzes.Received(1).AddAttemptAsync(Arg.Any<QuizAttempt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_learner_who_is_not_enrolled_is_refused()
    {
        Quiz quiz = Arrange(enrolled: false);

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.NotEnrolled);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_draft_quiz_cannot_be_attempted()
    {
        Quiz quiz = Arrange(status: AssessmentStatus.Draft);

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.NotPublished);
    }

    [Fact]
    public async Task Running_out_of_attempts_is_refused()
    {
        Quiz quiz = Arrange(maxAttempts: 2, submittedAttempts: 2);

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.NoAttemptsLeft);
        await _quizzes.DidNotReceive().AddAttemptAsync(Arg.Any<QuizAttempt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task The_next_attempt_is_numbered_after_the_ones_already_submitted()
    {
        Quiz quiz = Arrange(maxAttempts: 3, submittedAttempts: 2);

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.Value.AttemptNumber.Should().Be(3);
    }

    /// <summary>
    /// Reloading mid-quiz must resume rather than burn an attempt, and must give back the
    /// answers already saved.
    /// </summary>
    [Fact]
    public async Task An_open_attempt_is_resumed_rather_than_replaced()
    {
        Quiz quiz = Quiz.Create(_courseId, "Q", null, 30, 1, null, false, AssessmentStatus.Published);
        Question question = quiz.AddQuestion("Pick one", QuestionType.MultipleChoice, 10, 0, null);
        question.ReplaceOptions([("Wrong", false), ("Right", true)]);

        QuizAttempt open = QuizAttempt.Start(quiz.Id, _studentId, 1, Now.AddMinutes(-5));
        Guid chosen = question.Options.First().Id;
        open.Respond(question.Id, [chosen], null);

        Arrange(maxAttempts: 1, submittedAttempts: 0, openAttempt: open);
        _quizzes.GetQuizWithQuestionsAsync(quiz.Id, Arg.Any<CancellationToken>()).Returns(quiz);
        _quizzes.GetOpenAttemptAsync(quiz.Id, _studentId, Arg.Any<CancellationToken>()).Returns(open);

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AttemptId.Should().Be(open.Id);
        result.Value.Questions.Single().SelectedOptionIds.Should().ContainSingle().Which.Should().Be(chosen);
        await _quizzes.DidNotReceive().AddAttemptAsync(Arg.Any<QuizAttempt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task An_unauthenticated_caller_is_rejected()
    {
        Quiz quiz = Arrange();
        _currentUser.UserId.Returns((Guid?)null);

        Result<AttemptInProgressDto> result =
            await _sut.Handle(new StartAttemptCommand(quiz.Id), CancellationToken.None);

        result.Error.Should().Be(QuizErrors.Unauthenticated);
    }
}
