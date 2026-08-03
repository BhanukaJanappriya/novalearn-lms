using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Common.Errors;

/// <summary>Central catalogue of quiz failures.</summary>
public static class QuizErrors
{
    public static readonly Error QuizNotFound =
        Error.NotFound("quiz.not_found", "The requested quiz was not found.");

    public static readonly Error QuestionNotFound =
        Error.NotFound("quiz.question_not_found", "The requested question was not found.");

    public static readonly Error AttemptNotFound =
        Error.NotFound("quiz.attempt_not_found", "The requested attempt was not found.");

    public static readonly Error CourseNotFound =
        Error.NotFound("quiz.course_not_found", "The requested course was not found.");

    public static readonly Error Unauthenticated =
        Error.Unauthorized("quiz.unauthenticated", "You must be signed in to work with quizzes.");

    public static readonly Error NotCourseOwner =
        Error.Forbidden("quiz.not_course_owner", "You can only manage quizzes for courses that you own.");

    public static readonly Error NotEnrolled =
        Error.Forbidden("quiz.not_enrolled", "You must be enrolled in this course to take its quizzes.");

    public static readonly Error NotAttemptOwner =
        Error.Forbidden("quiz.not_attempt_owner", "You can only work on your own attempt.");

    public static readonly Error NotPublished =
        Error.Forbidden("quiz.not_published", "This quiz has not been published yet.");

    public static readonly Error NoAttemptsLeft =
        Error.Conflict("quiz.no_attempts_left", "You have used all your attempts at this quiz.");

    public static readonly Error AttemptAlreadySubmitted =
        Error.Conflict("quiz.attempt_already_submitted", "This attempt has already been submitted.");

    public static readonly Error AttemptStillOpen =
        Error.Conflict(
            "quiz.attempt_still_open",
            "You already have an attempt in progress. Finish it before starting another.");

    public static readonly Error AttemptNotSubmitted =
        Error.Conflict(
            "quiz.attempt_not_submitted",
            "This attempt has not been submitted yet, so there is no result to show.");

    public static readonly Error NotReadyToPublish =
        Error.Conflict(
            "quiz.not_ready_to_publish",
            "Add at least one question, and give every question a correct answer, before publishing.");

    public static readonly Error QuestionNotInAttempt =
        Error.Validation("quiz.question_not_in_attempt", "That question does not belong to this quiz.");

    public static readonly Error OptionRequired =
        Error.Validation(
            "quiz.option_required",
            "A multiple choice or true or false question needs at least two options and exactly one correct answer.");

    public static readonly Error AcceptedAnswerRequired =
        Error.Validation(
            "quiz.accepted_answer_required",
            "A short answer question needs at least one accepted answer.");
}
