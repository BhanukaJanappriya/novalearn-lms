using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Student.Dashboard;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Student;

public sealed class GetStudentDashboardQueryHandlerTests
{
    private readonly IStudentDashboardService _dashboard = Substitute.For<IStudentDashboardService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Guid _studentId = Guid.NewGuid();
    private readonly GetStudentDashboardQueryHandler _sut;

    public GetStudentDashboardQueryHandlerTests()
    {
        _sut = new GetStudentDashboardQueryHandler(_dashboard, _currentUser);
        _currentUser.UserId.Returns(_studentId);
    }

    private static StudentEnrollmentRow Row(
        string title,
        string category,
        int progress,
        string status = "Active",
        int lessons = 5,
        int minutes = 60,
        DateTimeOffset? enrolledAt = null) =>
        new(
            EnrollmentId: Guid.NewGuid(),
            CourseId: Guid.NewGuid(),
            CourseTitle: title,
            CourseCode: title.ToUpperInvariant()[..3],
            Category: category,
            Level: "Beginner",
            CoverImageUrl: null,
            LecturerName: "Nuwan Perera",
            Status: status,
            ProgressPercent: progress,
            EnrolledAtUtc: enrolledAt ?? DateTimeOffset.UtcNow.AddDays(-10),
            CompletedAtUtc: status == "Completed" ? DateTimeOffset.UtcNow : null,
            ModuleCount: 3,
            LessonCount: lessons,
            TotalMinutes: minutes,
            FirstLessonTitle: "Welcome");

    private void Arrange(params StudentEnrollmentRow[] rows) =>
        _dashboard.GetForStudentAsync(_studentId, Arg.Any<CancellationToken>())
            .Returns(new StudentStatistics(rows, [], []));

    [Fact]
    public async Task An_unauthenticated_caller_is_rejected()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        await _dashboard.DidNotReceive()
            .GetForStudentAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Courses_in_progress_are_ordered_closest_to_finishing_first()
    {
        Arrange(Row("Alpha", "Science", 20), Row("Beta", "Science", 80), Row("Gamma", "Science", 55));

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.Value.ContinueLearning.Select(c => c.Title)
            .Should().ContainInOrder("Beta", "Gamma", "Alpha");
    }

    [Fact]
    public async Task Completed_courses_are_separated_from_the_ones_still_running()
    {
        Arrange(Row("Alpha", "Science", 100, "Completed"), Row("Beta", "Science", 40));

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.Value.ContinueLearning.Should().ContainSingle(c => c.Title == "Beta");
        result.Value.Completed.Should().ContainSingle(c => c.Title == "Alpha");
        result.Value.Summary.ActiveCourses.Should().Be(1);
        result.Value.Summary.CompletedCourses.Should().Be(1);
    }

    [Fact]
    public async Task Average_progress_counts_completed_courses_in_the_denominator()
    {
        // 100 and 40 average to 70. Dropping the completed course would wrongly report 40.
        Arrange(Row("Alpha", "Science", 100, "Completed"), Row("Beta", "Science", 40));

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.Value.Summary.AverageProgressPercent.Should().Be(70);
    }

    [Fact]
    public async Task Nearly_done_counts_only_unfinished_courses_at_or_above_the_threshold()
    {
        Arrange(
            Row("Alpha", "Science", 74),
            Row("Beta", "Science", 75),
            Row("Gamma", "Science", 90),
            Row("Delta", "Science", 100, "Completed"));

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.Value.Summary.CoursesNearlyDone.Should().Be(2);
    }

    [Fact]
    public async Task Lesson_and_minute_totals_are_summed_across_every_enrolment()
    {
        Arrange(
            Row("Alpha", "Science", 10, lessons: 4, minutes: 30),
            Row("Beta", "Business", 20, lessons: 6, minutes: 45));

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.Value.Summary.LessonsAvailable.Should().Be(10);
        result.Value.Summary.LearningMinutes.Should().Be(75);
    }

    [Fact]
    public async Task Category_progress_averages_within_each_subject()
    {
        Arrange(
            Row("Alpha", "Science", 40),
            Row("Beta", "Science", 60),
            Row("Gamma", "Business", 90));

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        CategoryProgressDto science = result.Value.CategoryProgress.Single(c => c.Label == "Science");
        science.CourseCount.Should().Be(2);
        science.AverageProgressPercent.Should().Be(50);

        // Most-studied subject leads.
        result.Value.CategoryProgress.First().Label.Should().Be("Science");
    }

    [Fact]
    public async Task Activity_covers_a_dense_six_month_window_even_with_no_enrolments()
    {
        Arrange();

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.Value.EnrollmentActivity.Should().HaveCount(6);
        result.Value.EnrollmentActivity.Should().OnlyContain(p => p.Value == 0);
        result.Value.EnrollmentActivity.Last().Label
            .Should().Be(DateTimeOffset.UtcNow.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task An_empty_dashboard_reports_zeroes_rather_than_failing()
    {
        Arrange();

        Result<StudentDashboardResponse> result =
            await _sut.Handle(new GetStudentDashboardQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Summary.AverageProgressPercent.Should().Be(0);
        result.Value.ContinueLearning.Should().BeEmpty();
        result.Value.CategoryProgress.Should().BeEmpty();
    }
}
