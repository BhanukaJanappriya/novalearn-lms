using FluentAssertions;
using NSubstitute;
using NovaLearn.Application.Common.Errors;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Application.Features.Assessments.GetAssessmentOverview;
using NovaLearn.Domain.Assessments;
using NovaLearn.Domain.Identity;
using NovaLearn.Shared.Results;
using Xunit;

namespace NovaLearn.Application.UnitTests.Assessments;

public sealed class GetAssessmentOverviewQueryHandlerTests
{
    private readonly IAssessmentOverview _overview = Substitute.For<IAssessmentOverview>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly Guid _callerId = Guid.NewGuid();
    private readonly GetAssessmentOverviewQueryHandler _sut;

    private static readonly DateTimeOffset Now = new(2026, 8, 13, 9, 0, 0, TimeSpan.Zero);

    public GetAssessmentOverviewQueryHandlerTests()
    {
        _sut = new GetAssessmentOverviewQueryHandler(_overview, _currentUser, _clock);
        _clock.UtcNow.Returns(Now);
        Returns();
    }

    private void SignedInAs(params string[] roles)
    {
        _currentUser.UserId.Returns(_callerId);
        _currentUser.IsInRole(Arg.Any<string>()).Returns(call => roles.Contains(call.Arg<string>()));
    }

    private void Returns(params AssessmentOverviewRow[] rows) =>
        _overview.ListAsync(Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns<IReadOnlyList<AssessmentOverviewRow>>(rows);

    private static AssessmentOverviewRow Row(
        AssessmentKind kind = AssessmentKind.Assignment,
        AssessmentStatus status = AssessmentStatus.Published,
        DateTimeOffset? dueAtUtc = null,
        int awaitingMarking = 0) =>
        new(
            kind,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Organic Chemistry",
            "Titration write up",
            status,
            dueAtUtc,
            MaxPoints: 100,
            QuestionCount: 0,
            EnrolledCount: 20,
            SubmittedCount: 10,
            awaitingMarking,
            GradedCount: 5,
            AverageScorePercent: 71.5);

    private Task<Result<AssessmentOverviewDto>> Act() =>
        _sut.Handle(new GetAssessmentOverviewQuery(), CancellationToken.None);

    [Fact]
    public async Task An_anonymous_caller_is_refused()
    {
        _currentUser.UserId.Returns((Guid?)null);

        Result<AssessmentOverviewDto> result = await Act();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(AssessmentErrors.Unauthenticated);
    }

    [Fact]
    public async Task A_lecturer_only_ever_sees_their_own_courses()
    {
        SignedInAs(Roles.Lecturer);

        await Act();

        // The scope is a filter passed to the read model, not a check applied to its output, so
        // another lecturer's work never enters the result to begin with.
        await _overview.Received(1).ListAsync(_callerId, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(Roles.Administrator)]
    [InlineData(Roles.SuperAdministrator)]
    public async Task An_administrator_sees_every_course(string role)
    {
        SignedInAs(role);

        await Act();

        await _overview.Received(1).ListAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_lecturer_who_is_also_an_administrator_is_not_narrowed_to_their_own_courses()
    {
        SignedInAs(Roles.Lecturer, Roles.Administrator);

        await Act();

        await _overview.Received(1).ListAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Work_waiting_on_a_person_is_totalled_across_every_course()
    {
        SignedInAs(Roles.Lecturer);
        Returns(
            Row(awaitingMarking: 3),
            Row(kind: AssessmentKind.Quiz, awaitingMarking: 4),
            Row(awaitingMarking: 0));

        Result<AssessmentOverviewDto> result = await Act();

        result.Value.Summary.AwaitingMarking.Should().Be(7);
        result.Value.Summary.Total.Should().Be(3);
    }

    [Fact]
    public async Task Published_and_draft_work_are_counted_separately()
    {
        SignedInAs(Roles.Lecturer);
        Returns(
            Row(status: AssessmentStatus.Published),
            Row(status: AssessmentStatus.Published),
            Row(status: AssessmentStatus.Draft));

        Result<AssessmentOverviewDto> result = await Act();

        result.Value.Summary.Published.Should().Be(2);
        result.Value.Summary.Drafts.Should().Be(1);
    }

    [Fact]
    public async Task A_deadline_inside_the_next_week_is_due_soon()
    {
        SignedInAs(Roles.Lecturer);
        Returns(
            Row(dueAtUtc: Now.AddDays(2)),
            Row(dueAtUtc: Now.AddDays(6).AddHours(23)),
            Row(dueAtUtc: Now.AddDays(9)));

        Result<AssessmentOverviewDto> result = await Act();

        result.Value.Summary.DueSoon.Should().Be(2);
        result.Value.Summary.Overdue.Should().Be(0);
    }

    [Fact]
    public async Task A_deadline_in_the_past_is_overdue_rather_than_due_soon()
    {
        SignedInAs(Roles.Lecturer);
        Returns(Row(dueAtUtc: Now.AddDays(-1)));

        Result<AssessmentOverviewDto> result = await Act();

        result.Value.Summary.Overdue.Should().Be(1);
        result.Value.Summary.DueSoon.Should().Be(0);
    }

    [Fact]
    public async Task A_draft_is_never_due_or_overdue_however_its_date_reads()
    {
        SignedInAs(Roles.Lecturer);
        Returns(
            Row(status: AssessmentStatus.Draft, dueAtUtc: Now.AddDays(1)),
            Row(status: AssessmentStatus.Draft, dueAtUtc: Now.AddDays(-1)));

        Result<AssessmentOverviewDto> result = await Act();

        // Nobody can hand in a draft, so counting it as outstanding would invent work.
        result.Value.Summary.DueSoon.Should().Be(0);
        result.Value.Summary.Overdue.Should().Be(0);
    }

    [Fact]
    public async Task Undated_work_such_as_a_quiz_counts_as_neither()
    {
        SignedInAs(Roles.Lecturer);
        Returns(Row(kind: AssessmentKind.Quiz, dueAtUtc: null));

        Result<AssessmentOverviewDto> result = await Act();

        result.Value.Summary.DueSoon.Should().Be(0);
        result.Value.Summary.Overdue.Should().Be(0);
    }

    [Fact]
    public async Task An_empty_list_summarises_to_zeroes_rather_than_failing()
    {
        SignedInAs(Roles.Lecturer);

        Result<AssessmentOverviewDto> result = await Act();

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.Summary.Should().BeEquivalentTo(
            new AssessmentOverviewSummary(0, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void The_overview_row_carries_no_individual_learner_detail()
    {
        // The list is a workload view. Naming who scored what belongs to the per assignment and
        // per quiz screens, which check course ownership on the way in.
        string[] properties = typeof(AssessmentOverviewRow)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        properties.Should().NotContain(name =>
            name.Contains("Student", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Learner", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Feedback", StringComparison.OrdinalIgnoreCase));

        // What it does carry are tallies, which is what makes it safe to widen the audience later.
        properties.Should().Contain(nameof(AssessmentOverviewRow.AwaitingMarkingCount));
    }
}
