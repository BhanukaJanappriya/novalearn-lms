namespace NovaLearn.Domain.Quizzes;

/// <summary>Where a learner's attempt sits in its lifecycle.</summary>
public enum AttemptStatus
{
    /// <summary>Started and still open; answers can still be given.</summary>
    InProgress,

    /// <summary>Handed in and marked. Immutable from here.</summary>
    Submitted
}
