namespace NovaLearn.Domain.Quizzes;

/// <summary>Where a learner's attempt sits in its lifecycle.</summary>
public enum AttemptStatus
{
    /// <summary>Started and still open; answers can still be given.</summary>
    InProgress,

    /// <summary>
    /// Handed in and auto-marked, but it contains essay answers that still need a person.
    /// The score shown is provisional until they are marked.
    /// </summary>
    PendingReview,

    /// <summary>Fully marked. The score is final and the attempt is immutable.</summary>
    Graded
}
