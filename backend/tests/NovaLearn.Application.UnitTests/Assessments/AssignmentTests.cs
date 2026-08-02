using FluentAssertions;
using NovaLearn.Domain.Assessments;
using Xunit;

namespace NovaLearn.Application.UnitTests.Assessments;

public sealed class AssignmentTests
{
    private static readonly DateTimeOffset Due = new(2026, 8, 10, 12, 0, 0, TimeSpan.Zero);

    private static Assignment Create(
        DateTimeOffset? due = null,
        bool allowLate = false,
        AssessmentStatus status = AssessmentStatus.Published,
        int maxPoints = 100) =>
        Assignment.Create(Guid.NewGuid(), "  Problem set 1  ", "  Do the work  ", due, maxPoints, allowLate, status);

    [Fact]
    public void Text_is_trimmed_on_creation()
    {
        Assignment assignment = Create();

        assignment.Title.Should().Be("Problem set 1");
        assignment.Instructions.Should().Be("Do the work");
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-50, 1)]
    [InlineData(5000, Assignment.MaxPointsCeiling)]
    [InlineData(50, 50)]
    public void Points_are_clamped_to_a_sane_scale(int requested, int expected)
    {
        Create(maxPoints: requested).MaxPoints.Should().Be(expected);
    }

    [Fact]
    public void Blank_instructions_become_null_rather_than_empty()
    {
        Assignment assignment =
            Assignment.Create(Guid.NewGuid(), "T", "   ", null, 10, false, AssessmentStatus.Draft);

        assignment.Instructions.Should().BeNull();
    }

    [Fact]
    public void Work_with_no_due_date_is_never_late()
    {
        Assignment assignment = Create(due: null);

        assignment.IsLateAt(DateTimeOffset.UtcNow.AddYears(5)).Should().BeFalse();
        assignment.AcceptsSubmissionAt(DateTimeOffset.UtcNow.AddYears(5)).Should().BeTrue();
    }

    [Fact]
    public void Work_handed_in_after_the_due_date_is_late()
    {
        Assignment assignment = Create(due: Due);

        assignment.IsLateAt(Due.AddMinutes(-1)).Should().BeFalse();
        assignment.IsLateAt(Due.AddMinutes(1)).Should().BeTrue();
    }

    [Fact]
    public void A_closed_assignment_refuses_late_work()
    {
        Assignment assignment = Create(due: Due, allowLate: false);

        assignment.AcceptsSubmissionAt(Due.AddMinutes(-1)).Should().BeTrue();
        assignment.AcceptsSubmissionAt(Due.AddMinutes(1)).Should().BeFalse();
    }

    [Fact]
    public void An_assignment_that_allows_late_work_stays_open()
    {
        Assignment assignment = Create(due: Due, allowLate: true);

        assignment.AcceptsSubmissionAt(Due.AddDays(30)).Should().BeTrue();
    }

    [Fact]
    public void A_draft_assignment_is_closed_to_everyone()
    {
        Assignment assignment = Create(due: null, status: AssessmentStatus.Draft);

        assignment.AcceptsSubmissionAt(DateTimeOffset.UtcNow).Should().BeFalse();

        assignment.Publish();
        assignment.AcceptsSubmissionAt(DateTimeOffset.UtcNow).Should().BeTrue();

        assignment.Unpublish();
        assignment.AcceptsSubmissionAt(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Update_applies_the_same_invariants_as_creation()
    {
        Assignment assignment = Create();

        assignment.Update("  Renamed  ", "   ", null, 99999, true, AssessmentStatus.Draft);

        assignment.Title.Should().Be("Renamed");
        assignment.Instructions.Should().BeNull();
        assignment.MaxPoints.Should().Be(Assignment.MaxPointsCeiling);
        assignment.AllowLateSubmissions.Should().BeTrue();
        assignment.Status.Should().Be(AssessmentStatus.Draft);
    }
}
