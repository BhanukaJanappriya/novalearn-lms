using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using NovaLearn.API.Common;
using NovaLearn.Application.Features.Quizzes.Common;
using NovaLearn.Application.Features.Quizzes.CreateQuiz;
using NovaLearn.Application.Features.Quizzes.DeleteQuestion;
using NovaLearn.Application.Features.Quizzes.DeleteQuiz;
using NovaLearn.Application.Features.Quizzes.DuplicateQuestion;
using NovaLearn.Application.Features.Quizzes.GetAttemptResult;
using NovaLearn.Application.Features.Quizzes.GetCourseQuizzes;
using NovaLearn.Application.Features.Quizzes.GetQuizForAuthoring;
using NovaLearn.Application.Features.Quizzes.GetQuizResults;
using NovaLearn.Application.Features.Quizzes.MarkEssayAnswer;
using NovaLearn.Application.Features.Quizzes.ReorderQuestions;
using NovaLearn.Application.Features.Quizzes.SaveAnswer;
using NovaLearn.Application.Features.Quizzes.SaveQuestion;
using NovaLearn.Application.Features.Quizzes.StartAttempt;
using NovaLearn.Application.Features.Quizzes.SubmitAttempt;
using NovaLearn.Application.Features.Quizzes.UpdateQuiz;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;

namespace NovaLearn.API.Features.Quizzes;

/// <summary>
/// Quiz authoring and the attempt loop. Routes span the <c>courses</c>, <c>quizzes</c>,
/// <c>questions</c> and <c>attempts</c> segments, so each action declares its own.
///
/// The authoring endpoint is the one that returns correct answers, so it carries both the role
/// gate and the course-ownership check. Taking a quiz is open to any authenticated caller
/// because the real gate is an active enrolment.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}")]
[Authorize]
public sealed class QuizzesController(ISender sender) : ApiControllerBase
{
    private const string ManagerRoles =
        $"{Roles.Lecturer},{Roles.Administrator},{Roles.SuperAdministrator}";

    /// <summary>Lists a course's quizzes. Carries no question content for either audience.</summary>
    [HttpGet("courses/{courseId:guid}/quizzes")]
    [ProducesResponseType(typeof(IReadOnlyList<QuizSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListQuizzes(Guid courseId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetCourseQuizzesQuery(courseId), cancellationToken));

    /// <summary>Creates a quiz on a course you own. New quizzes always start as drafts.</summary>
    [HttpPost("courses/{courseId:guid}/quizzes")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(QuizSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateQuiz(
        Guid courseId, CreateQuizRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateQuizCommand(
            courseId, request.Title, request.Description, request.TimeLimitMinutes,
            request.MaxAttempts, request.PassingScorePercent, request.ShuffleQuestions, request.Status);

        Result<QuizSummaryDto> result = await sender.Send(command, cancellationToken);

        return HandleResult(
            result, quiz => CreatedAtAction(nameof(ListQuizzes), new { courseId }, quiz));
    }

    /// <summary>The full quiz including correct answers. Course owner or admin only.</summary>
    [HttpGet("quizzes/{quizId:guid}/authoring")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(QuizAuthoringDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForAuthoring(Guid quizId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetQuizForAuthoringQuery(quizId), cancellationToken));

    /// <summary>Edits a quiz. Publishing is refused unless every question is answerable.</summary>
    [HttpPut("quizzes/{quizId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(QuizSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateQuiz(
        Guid quizId, UpdateQuizRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateQuizCommand(
            quizId, request.Title, request.Description, request.TimeLimitMinutes,
            request.MaxAttempts, request.PassingScorePercent, request.ShuffleQuestions, request.Status);

        return HandleResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>Deletes a quiz and its questions. Recorded attempts are kept.</summary>
    [HttpDelete("quizzes/{quizId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuiz(Guid quizId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteQuizCommand(quizId), cancellationToken));

    /// <summary>Creates or replaces a question, options and all.</summary>
    [HttpPut("quizzes/{quizId:guid}/questions")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(AuthoringQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveQuestion(
        Guid quizId, SaveQuestionRequest request, CancellationToken cancellationToken)
    {
        var command = new SaveQuestionCommand(
            quizId,
            request.QuestionId,
            request.Text,
            request.Type,
            request.Points,
            request.AcceptedAnswers ?? [],
            request.Options ?? [],
            request.IsRequired,
            request.MarkingGuidance);

        return HandleResult(await sender.Send(command, cancellationToken));
    }

    /// <summary>Deletes a question and its options.</summary>
    [HttpDelete("questions/{questionId:guid}")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion(Guid questionId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DeleteQuestionCommand(questionId), cancellationToken));

    /// <summary>How the cohort did. Course owner or admin only.</summary>
    [HttpGet("quizzes/{quizId:guid}/results")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(QuizResultsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Results(Guid quizId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetQuizResultsQuery(quizId), cancellationToken));

    /// <summary>Starts a sitting, or resumes the one already open. Requires an active enrolment.</summary>
    [HttpPost("quizzes/{quizId:guid}/attempts")]
    [ProducesResponseType(typeof(AttemptInProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartAttempt(Guid quizId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new StartAttemptCommand(quizId), cancellationToken));

    /// <summary>Records one answer while the attempt is open.</summary>
    [HttpPut("attempts/{attemptId:guid}/answers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SaveAnswer(
        Guid attemptId, SaveAnswerRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new SaveAnswerCommand(
                attemptId, request.QuestionId, request.SelectedOptionIds ?? [], request.TextAnswer),
            cancellationToken));

    /// <summary>Hands the attempt in and marks it.</summary>
    [HttpPost("attempts/{attemptId:guid}/submit")]
    [ProducesResponseType(typeof(AttemptResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SubmitAttempt(Guid attemptId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new SubmitAttemptCommand(attemptId), cancellationToken));

    /// <summary>Reorders a quiz's questions. Must list every question id.</summary>
    [HttpPut("quizzes/{quizId:guid}/questions/order")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(QuizAuthoringDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderQuestions(
        Guid quizId, ReorderQuestionsRequest request, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new ReorderQuestionsCommand(quizId, request.QuestionIds), cancellationToken));

    /// <summary>Copies a question, answer key and all, to the end of its quiz.</summary>
    [HttpPost("questions/{questionId:guid}/duplicate")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(AuthoringQuestionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DuplicateQuestion(
        Guid questionId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new DuplicateQuestionCommand(questionId), cancellationToken));

    /// <summary>Marks one essay answer by hand. Course owner or admin only.</summary>
    [HttpPut("attempts/{attemptId:guid}/answers/{answerId:guid}/mark")]
    [Authorize(Roles = ManagerRoles)]
    [ProducesResponseType(typeof(AttemptResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MarkEssayAnswer(
        Guid attemptId,
        Guid answerId,
        MarkEssayAnswerRequest request,
        CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(
            new MarkEssayAnswerCommand(attemptId, answerId, request.PointsAwarded, request.Feedback),
            cancellationToken));

    /// <summary>A marked attempt. The learner who sat it, or staff on that course.</summary>
    [HttpGet("attempts/{attemptId:guid}")]
    [ProducesResponseType(typeof(AttemptResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AttemptResult(Guid attemptId, CancellationToken cancellationToken) =>
        HandleResult(await sender.Send(new GetAttemptResultQuery(attemptId), cancellationToken));
}
