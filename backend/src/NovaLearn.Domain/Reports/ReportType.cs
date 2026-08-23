namespace NovaLearn.Domain.Reports;

/// <summary>Which report was run. Drives both the query and the audit trail entry it leaves.</summary>
public enum ReportType
{
    Enrollments,
    Revenue,
    CoursePerformance,
    Users,
    SupportTickets
}
