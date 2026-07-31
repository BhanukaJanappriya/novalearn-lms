namespace NovaLearn.Application.Features.Student.Dashboard;

/// <summary>
/// The learner dashboard aggregate. Property names serialise to camelCase and match the
/// frontend's <c>StudentDashboard</c> TypeScript contract one-to-one, so the client consumes
/// this with no transformation.
/// </summary>
public sealed record StudentDashboardResponse(
    StudentSummaryDto Summary,
    IReadOnlyList<StudentCourseDto> ContinueLearning,
    IReadOnlyList<StudentCourseDto> Completed,
    IReadOnlyList<CategoryProgressDto> CategoryProgress,
    IReadOnlyList<ActivityPointDto> EnrollmentActivity,
    IReadOnlyList<RecommendedCourseDto> Recommended);

/// <summary>Headline counters. All derived from the learner's own enrolments.</summary>
public sealed record StudentSummaryDto(
    int ActiveCourses,
    int CompletedCourses,
    int AverageProgressPercent,
    int LessonsAvailable,
    int LearningMinutes,
    int CoursesNearlyDone);

/// <summary>An enrolled course with its progress and content scale.</summary>
public sealed record StudentCourseDto(
    Guid EnrollmentId,
    Guid CourseId,
    string Title,
    string Code,
    string Category,
    string Level,
    string? CoverImageUrl,
    string LecturerName,
    string Status,
    int ProgressPercent,
    int ModuleCount,
    int LessonCount,
    int TotalMinutes,
    string? FirstLessonTitle,
    DateTimeOffset EnrolledAtUtc,
    DateTimeOffset? CompletedAtUtc);

/// <summary>Average progress across the learner's courses in one subject area.</summary>
public sealed record CategoryProgressDto(string Label, int CourseCount, int AverageProgressPercent);

/// <summary>One month of the learner's enrolment activity.</summary>
public sealed record ActivityPointDto(string Label, int Value);

/// <summary>A published course the learner has not joined yet.</summary>
public sealed record RecommendedCourseDto(
    Guid CourseId,
    string Title,
    string Code,
    string Category,
    string Level,
    decimal Price,
    string? CoverImageUrl,
    string LecturerName,
    int EnrolledCount,
    int LessonCount);
