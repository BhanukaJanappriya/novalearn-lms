using FluentAssertions;
using NovaLearn.Domain.Reports;
using Xunit;

namespace NovaLearn.Application.UnitTests.Reports;

public sealed class ReportRunTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 23, 12, 0, 0, TimeSpan.Zero);
    private readonly Guid _staffId = Guid.NewGuid();

    [Fact]
    public void Creating_a_run_stamps_everything_passed_in()
    {
        ReportRun run = ReportRun.Create(ReportType.Enrollments, _staffId, "status=Active", 42, Now);

        run.Type.Should().Be(ReportType.Enrollments);
        run.GeneratedById.Should().Be(_staffId);
        run.FiltersSummary.Should().Be("status=Active");
        run.RowCount.Should().Be(42);
        run.CreatedAtUtc.Should().Be(Now);
    }

    [Fact]
    public void Blank_filters_are_stored_as_null_rather_than_an_empty_string()
    {
        ReportRun run = ReportRun.Create(ReportType.Users, _staffId, "   ", 0, Now);

        run.FiltersSummary.Should().BeNull();
    }

    [Fact]
    public void Filters_are_trimmed()
    {
        ReportRun run = ReportRun.Create(ReportType.Revenue, _staffId, "  status=Succeeded  ", 5, Now);

        run.FiltersSummary.Should().Be("status=Succeeded");
    }
}
