namespace NovaLearn.Domain.Assessments;

/// <summary>Publication state of an assessment, mirroring <c>CourseStatus</c>.</summary>
public enum AssessmentStatus
{
    /// <summary>Being authored; invisible to learners.</summary>
    Draft,

    /// <summary>Visible to enrolled learners and open for submission.</summary>
    Published
}
