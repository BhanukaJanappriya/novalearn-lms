namespace NovaLearn.Domain.Courses;

/// <summary>Publication state of a course.</summary>
public enum CourseStatus
{
    /// <summary>Work in progress; not visible to learners.</summary>
    Draft,

    /// <summary>Live and open for enrolment.</summary>
    Published
}
