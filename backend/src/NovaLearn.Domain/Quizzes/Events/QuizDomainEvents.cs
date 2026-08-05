using NovaLearn.Domain.Common;

namespace NovaLearn.Domain.Quizzes.Events;

/// <summary>
/// Raised when a quiz first becomes available to learners. Only on the transition, so editing a
/// live quiz does not notify everyone again.
/// </summary>
public sealed record QuizPublishedDomainEvent(
    Guid QuizId,
    Guid CourseId,
    string Title,
    int QuestionCount,
    int? TimeLimitMinutes) : DomainEvent;
