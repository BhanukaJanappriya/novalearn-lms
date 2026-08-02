namespace NovaLearn.Domain.Assessments;

/// <summary>Where a learner's submission sits in the marking cycle.</summary>
public enum SubmissionStatus
{
    /// <summary>Handed in, awaiting marking.</summary>
    Submitted,

    /// <summary>Marked, with points and optional feedback recorded.</summary>
    Graded
}
