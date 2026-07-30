using FluentAssertions;
using NovaLearn.Domain.Enrollments;
using Xunit;

namespace NovaLearn.Application.UnitTests.Enrollments;

public sealed class EnrollmentTests
{
    private static readonly DateTimeOffset EnrolledAt = new(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);

    private static Enrollment NewEnrollment() =>
        Enrollment.Create(Guid.NewGuid(), Guid.NewGuid(), EnrolledAt);

    [Fact]
    public void Create_starts_active_at_zero_percent()
    {
        Enrollment enrollment = NewEnrollment();

        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.ProgressPercent.Should().Be(0);
        enrollment.EnrolledAtUtc.Should().Be(EnrolledAt);
        enrollment.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateProgress_to_100_completes_and_stamps_the_completion_time()
    {
        Enrollment enrollment = NewEnrollment();
        DateTimeOffset completedAt = EnrolledAt.AddDays(30);

        enrollment.UpdateProgress(100, completedAt);

        enrollment.ProgressPercent.Should().Be(100);
        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
        enrollment.CompletedAtUtc.Should().Be(completedAt);
    }

    [Fact]
    public void UpdateProgress_below_100_leaves_the_enrollment_active()
    {
        Enrollment enrollment = NewEnrollment();

        enrollment.UpdateProgress(64);

        enrollment.ProgressPercent.Should().Be(64);
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public void UpdateProgress_reopens_a_completed_enrollment_when_progress_falls_back()
    {
        Enrollment enrollment = NewEnrollment();
        enrollment.UpdateProgress(100, EnrolledAt.AddDays(10));

        enrollment.UpdateProgress(80);

        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.CompletedAtUtc.Should().BeNull();
    }

    [Theory]
    [InlineData(-40, 0)]
    [InlineData(0, 0)]
    [InlineData(55, 55)]
    [InlineData(100, 100)]
    [InlineData(180, 100)]
    public void UpdateProgress_clamps_to_the_valid_range(int input, int expected)
    {
        Enrollment enrollment = NewEnrollment();

        enrollment.UpdateProgress(input, EnrolledAt);

        enrollment.ProgressPercent.Should().Be(expected);
    }

    [Fact]
    public void Drop_marks_the_enrollment_dropped_without_losing_progress()
    {
        Enrollment enrollment = NewEnrollment();
        enrollment.UpdateProgress(45);

        enrollment.Drop();

        enrollment.Status.Should().Be(EnrollmentStatus.Dropped);
        enrollment.ProgressPercent.Should().Be(45);
    }

    [Fact]
    public void Reactivate_returns_a_dropped_enrollment_to_its_progress_based_status()
    {
        Enrollment inProgress = NewEnrollment();
        inProgress.UpdateProgress(45);
        inProgress.Drop();

        Enrollment finished = NewEnrollment();
        finished.UpdateProgress(100, EnrolledAt.AddDays(5));
        finished.Drop();

        inProgress.Reactivate();
        finished.Reactivate();

        inProgress.Status.Should().Be(EnrollmentStatus.Active);
        finished.Status.Should().Be(EnrollmentStatus.Completed);
    }
}
