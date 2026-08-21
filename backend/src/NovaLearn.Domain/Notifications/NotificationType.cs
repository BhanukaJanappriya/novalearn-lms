namespace NovaLearn.Domain.Notifications;

/// <summary>
/// What a notification is about. Kept coarse: the client picks an icon and colour from this, so
/// adding a value is a frontend change too.
/// </summary>
public enum NotificationType
{
    /// <summary>A learner's submitted work has been marked.</summary>
    SubmissionGraded,

    /// <summary>New assessed work has been published on a course the learner is on.</summary>
    AssignmentPublished,

    /// <summary>A new quiz has been published on a course the learner is on.</summary>
    QuizPublished,

    /// <summary>A learner has handed work in, so the course owner has something to mark.</summary>
    SubmissionReceived,

    /// <summary>A learner's payment for a course was confirmed and the enrolment created.</summary>
    PaymentSucceeded,

    /// <summary>A support ticket the recipient cares about received a new reply.</summary>
    SupportTicketReplied,

    /// <summary>A support ticket's status changed.</summary>
    SupportTicketStatusChanged
}
