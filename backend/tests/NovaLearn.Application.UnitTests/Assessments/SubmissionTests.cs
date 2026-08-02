using FluentAssertions;
using NovaLearn.Domain.Assessments;
using Xunit;

namespace NovaLearn.Application.UnitTests.Assessments;

public sealed class SubmissionTests
{
    private static readonly DateTimeOffset SubmittedAt = new(2026, 8, 1, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset GradedAt = new(2026, 8, 2, 9, 0, 0, TimeSpan.Zero);

    private static Submission Create(bool isLate = false) =>
        Submission.Create(Guid.NewGuid(), Guid.NewGuid(), "  my answer  ", "  https://x.dev  ", SubmittedAt, isLate);

    [Fact]
    public void A_new_submission_starts_unmarked()
    {
        Submission submission = Create();

        submission.Status.Should().Be(SubmissionStatus.Submitted);
        submission.PointsAwarded.Should().BeNull();
        submission.GradedAtUtc.Should().BeNull();
        submission.Content.Should().Be("my answer");
        submission.AttachmentUrl.Should().Be("https://x.dev");
    }

    [Theory]
    [InlineData(50, 100, 50)]
    [InlineData(150, 100, 100)]
    [InlineData(-10, 100, 0)]
    public void Points_are_clamped_to_what_the_assignment_is_worth(int requested, int max, int expected)
    {
        Submission submission = Create();

        submission.Grade(requested, "good", max, Guid.NewGuid(), GradedAt);

        submission.PointsAwarded.Should().Be(expected);
    }

    [Fact]
    public void Grading_records_who_marked_it_and_when()
    {
        Submission submission = Create();
        Guid grader = Guid.NewGuid();

        submission.Grade(80, "  well done  ", 100, grader, GradedAt);

        submission.Status.Should().Be(SubmissionStatus.Graded);
        submission.Feedback.Should().Be("well done");
        submission.GradedById.Should().Be(grader);
        submission.GradedAtUtc.Should().Be(GradedAt);
    }

    /// <summary>
    /// The important rule: a learner must never keep a mark that was awarded for work they have
    /// since replaced.
    /// </summary>
    [Fact]
    public void Resubmitting_discards_the_previous_mark()
    {
        Submission submission = Create();
        submission.Grade(80, "well done", 100, Guid.NewGuid(), GradedAt);

        submission.Resubmit("a better answer", null, GradedAt.AddDays(1), isLate: true);

        submission.Status.Should().Be(SubmissionStatus.Submitted);
        submission.PointsAwarded.Should().BeNull();
        submission.Feedback.Should().BeNull();
        submission.GradedById.Should().BeNull();
        submission.GradedAtUtc.Should().BeNull();

        submission.Content.Should().Be("a better answer");
        submission.AttachmentUrl.Should().BeNull();
        submission.IsLate.Should().BeTrue();
    }

    [Fact]
    public void The_late_flag_is_captured_at_hand_in_time()
    {
        Submission submission = Create(isLate: true);

        submission.IsLate.Should().BeTrue();
    }
}
