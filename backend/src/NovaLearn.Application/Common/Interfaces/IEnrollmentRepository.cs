using NovaLearn.Application.Features.Enrollments.Common;
using NovaLearn.Domain.Courses;
using NovaLearn.Domain.Enrollments;
using NovaLearn.Shared.Common;

namespace NovaLearn.Application.Common.Interfaces;

/// <summary>Persistence port for the <see cref="Enrollment"/> aggregate.</summary>
public interface IEnrollmentRepository
{
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken);

    /// <summary>Loads an enrolment (with its student and course) by id, or null.</summary>
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    void Remove(Enrollment enrollment);

    /// <summary>The student's non-dropped enrolment in a course, if one exists.</summary>
    Task<Enrollment?> GetActiveAsync(Guid studentId, Guid courseId, CancellationToken cancellationToken);

    /// <summary>A student's enrolments, newest first, with the course included.</summary>
    Task<IReadOnlyList<Enrollment>> ListForStudentAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>A course's roster, newest first, with the student included.</summary>
    Task<IReadOnlyList<Enrollment>> ListForCourseAsync(Guid courseId, CancellationToken cancellationToken);

    /// <summary>
    /// The learner-facing catalogue: published courses only, optionally filtered, with each
    /// course's active enrolment count and whether <paramref name="studentId"/> has joined it.
    /// Lives on this port because the projection is driven by enrolment data.
    /// </summary>
    Task<PagedResult<CourseCatalogDto>> GetCatalogAsync(
        string? search,
        string? category,
        CourseLevel? level,
        Guid? studentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
