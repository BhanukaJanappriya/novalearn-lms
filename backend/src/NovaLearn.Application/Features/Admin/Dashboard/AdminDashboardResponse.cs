namespace NovaLearn.Application.Features.Admin.Dashboard;

/// <summary>
/// The admin dashboard aggregate. Property names serialise to camelCase and match the
/// frontend's <c>AdminDashboard</c> TypeScript contract one-to-one, so the client consumes
/// this with no transformation.
/// </summary>
public sealed record AdminDashboardResponse(
    ExecutiveSummaryDto Summary,
    IReadOnlyList<KpiMetricDto> Kpis,
    IReadOnlyList<TimeSeriesPointDto> EnrollmentTrend,
    IReadOnlyList<TimeSeriesPointDto> CompletionTrend,
    IReadOnlyList<CategoryDatumDto> RoleDistribution,
    IReadOnlyList<TimeSeriesPointDto> WeeklyActivity,
    IReadOnlyList<CategoryDatumDto> PopularCourses,
    IReadOnlyList<ActivityItemDto> Activity,
    IReadOnlyList<PendingApprovalDto> Approvals,
    SystemHealthDto Health,
    SecuritySummaryDto Security,
    IReadOnlyList<AiInsightDto> Insights);

public sealed record ExecutiveSummaryDto(
    string SemesterName,
    int SemesterProgressPct,
    string SemesterStart,
    string SemesterEnd,
    int ActiveUsersNow,
    double ServerHealthPct,
    int PendingApprovals,
    string SystemStatus,
    string AcademicPeriod);

public sealed record KpiMetricDto(
    string Id,
    string Label,
    double Value,
    string Format,
    double DeltaPct,
    string Trend,
    bool HigherIsBetter,
    string Icon,
    IReadOnlyList<string> Accent,
    IReadOnlyList<double> Spark,
    string? Href,
    string? Hint);

public sealed record TimeSeriesPointDto(string Label, double Value, double? Compare);

public sealed record CategoryDatumDto(string Label, double Value);

public sealed record ActivityItemDto(
    string Id,
    string Category,
    string ActorName,
    string ActorColor,
    string Message,
    string Status,
    DateTimeOffset Timestamp);

public sealed record PendingApprovalDto(
    string Id,
    string Kind,
    string Name,
    string Subtitle,
    string Color,
    DateTimeOffset SubmittedAt,
    string Meta);

public sealed record SystemHealthDto(
    IReadOnlyList<SystemServiceDto> Services,
    IReadOnlyList<SystemResourceDto> Resources);

public sealed record SystemServiceDto(string Name, string Status, int LatencyMs, double UptimePct);

public sealed record SystemResourceDto(string Name, int UsagePct);

public sealed record SecuritySummaryDto(
    int Score,
    int FailedLogins24h,
    int BlockedIps,
    int ActiveSessions,
    int TwoFactorAdoptionPct,
    IReadOnlyList<SecurityEventDto> Events);

public sealed record SecurityEventDto(
    string Id,
    string Label,
    string Detail,
    string Severity,
    DateTimeOffset Timestamp);

public sealed record AiInsightDto(
    string Id,
    string Severity,
    string Title,
    string Body,
    string? Metric,
    string ActionLabel);
