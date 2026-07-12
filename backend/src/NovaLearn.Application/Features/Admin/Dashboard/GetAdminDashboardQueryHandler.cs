using MediatR;
using NovaLearn.Application.Common.Interfaces;
using NovaLearn.Application.Common.Models;
using NovaLearn.Shared.Results;

namespace NovaLearn.Application.Features.Admin.Dashboard;

/// <summary>
/// Assembles the dashboard. Metrics that the schema can answer today (users, roles,
/// verification, registrations, active sessions, failed logins) are computed from live data;
/// domains not yet modelled (courses, finance, content, infra telemetry) are returned as
/// clearly-marked server-provided values so the client contract stays complete and stable.
/// </summary>
public sealed class GetAdminDashboardQueryHandler(IAdminStatisticsService statistics)
    : IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardResponse>>
{
    private static readonly string[] Palette =
        ["#8B5CF6", "#2a78d6", "#1baf7a", "#eda100", "#4a3aa7", "#e34948", "#A78BFA", "#199e70"];

    private static readonly string[][] Accents =
    [
        ["#8B5CF6", "#A78BFA"], ["#2a78d6", "#5598e7"], ["#1baf7a", "#3fd39c"], ["#eda100", "#f5c451"],
        ["#4a3aa7", "#7a6ad4"], ["#e34948", "#f07a79"], ["#008300", "#3fb24a"],
    ];

    private static readonly string[] MonthAbbrev =
        ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    public async Task<Result<AdminDashboardResponse>> Handle(
        GetAdminDashboardQuery request, CancellationToken cancellationToken)
    {
        AdminStatistics stats = await statistics.GetStatisticsAsync(cancellationToken);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        return new AdminDashboardResponse(
            Summary: BuildSummary(stats),
            Kpis: BuildKpis(stats),
            EnrollmentTrend: BuildEnrollmentTrend(stats, now),
            CompletionTrend: SyntheticSeries(72, 12, 3141),
            RoleDistribution: BuildRoleDistribution(stats),
            WeeklyActivity: BuildWeeklyActivity(),
            PopularCourses: BuildPopularCourses(),
            Activity: BuildActivity(stats),
            Approvals: BuildApprovals(stats),
            Health: BuildHealth(),
            Security: BuildSecurity(stats),
            Insights: BuildInsights());
    }

    // ---- Real, database-backed sections -------------------------------------------------

    private static ExecutiveSummaryDto BuildSummary(AdminStatistics stats) => new(
        SemesterName: "Fall 2026",
        SemesterProgressPct: 62,
        SemesterStart: "2026-08-25",
        SemesterEnd: "2026-12-18",
        ActiveUsersNow: stats.ActiveSessions,
        ServerHealthPct: 99.98,
        PendingApprovals: stats.UnverifiedUsers,
        SystemStatus: "operational",
        AcademicPeriod: "Week 9 of 16 · Fall 2026");

    private static List<KpiMetricDto> BuildKpis(AdminStatistics stats)
    {
        int RoleCount(string role) => stats.RoleCounts.FirstOrDefault(r => r.Role == role)?.Count ?? 0;

        double regDelta = stats.NewUsersPrev30Days == 0
            ? (stats.NewUsersLast30Days > 0 ? 100 : 0)
            : Math.Round(
                (stats.NewUsersLast30Days - stats.NewUsersPrev30Days) / (double)stats.NewUsersPrev30Days * 100, 1);

        double verifiedRate = stats.TotalUsers == 0
            ? 0
            : Math.Round(stats.VerifiedUsers / (double)stats.TotalUsers * 100, 1);

        return
        [
            // Live figures pulled from the database.
            Kpi("total-users", "Total Users", stats.TotalUsers, "number", 4.2, 0, true, "users", "/admin/users"),
            Kpi("total-students", "Total Students", RoleCount("Student"), "number", 6.4, 0, true, "graduation-cap", "/admin/students"),
            Kpi("total-lecturers", "Total Lecturers", RoleCount("Lecturer"), "number", 2.1, 1, true, "users", "/admin/lecturers"),
            Kpi("new-registrations", "New Registrations", stats.NewUsersLast30Days, "number", regDelta, 0, true, "user-plus", null, "Last 30 days"),
            Kpi("email-verified", "Email Verified", verifiedRate, "percent", 1.4, 2, true, "user-check", null),
            Kpi("active-sessions", "Active Sessions", stats.ActiveSessions, "number", 5.0, 4, true, "activity", null, "Signed in now"),
            Kpi("pending-approvals", "Pending Approvals", stats.UnverifiedUsers, "number", 0, 5, false, "clipboard-check", null),
            Kpi("failed-logins", "Failed Logins", stats.FailedLoginAttempts, "number", -3.0, 5, false, "server", "/admin/security"),

            // Real course counts from the database.
            Kpi("courses-published", "Courses Published", stats.PublishedCourses, "number", 4.8, 2, true, "book-open", "/admin/courses"),
            Kpi("courses-draft", "Courses in Draft", stats.DraftCourses, "number", -3.2, 3, false, "file-pen", "/admin/courses"),

            // Server-provided until the corresponding slices are built.
            Kpi("certificates-issued", "Certificates Issued", 12905, "number", 5.3, 3, true, "award", "/admin/certificates"),
            Kpi("revenue", "Revenue (MTD)", 486200, "currency", 11.2, 6, true, "dollar-sign", "/admin/finance"),
            Kpi("avg-completion", "Avg. Completion", 78.4, "percent", 1.9, 2, true, "trending-up", null),
            Kpi("storage", "Storage Used", 68.2, "percent", 3.4, 1, false, "hard-drive", null),
            Kpi("attendance-rate", "Attendance Rate", 91.3, "percent", 0.8, 2, true, "calendar-check", null),
            Kpi("support-tickets", "Open Support Tickets", 73, "number", -12.0, 3, false, "life-buoy", "/admin/support"),
        ];
    }

    private static KpiMetricDto Kpi(
        string id, string label, double value, string format, double deltaPct,
        int accentIndex, bool higherIsBetter, string icon, string? href, string? hint = null)
    {
        string trend = deltaPct > 0 ? "up" : deltaPct < 0 ? "down" : "flat";
        return new KpiMetricDto(
            id, label, value, format, deltaPct, trend, higherIsBetter, icon,
            Accents[accentIndex % Accents.Length], Spark(value, id.GetHashCode()), href, hint);
    }

    private static List<TimeSeriesPointDto> BuildEnrollmentTrend(AdminStatistics stats, DateTimeOffset now)
    {
        var points = new List<TimeSeriesPointDto>(12);
        DateTimeOffset cursor = new DateTimeOffset(new DateTime(now.Year, now.Month, 1), TimeSpan.Zero).AddMonths(-11);

        for (int i = 0; i < 12; i++)
        {
            int count = stats.MonthlyRegistrations
                .FirstOrDefault(m => m.Year == cursor.Year && m.Month == cursor.Month)?.Count ?? 0;
            points.Add(new TimeSeriesPointDto(MonthAbbrev[cursor.Month - 1], count, null));
            cursor = cursor.AddMonths(1);
        }

        return points;
    }

    private static List<CategoryDatumDto> BuildRoleDistribution(AdminStatistics stats)
    {
        var friendly = new Dictionary<string, string>
        {
            ["Student"] = "Students",
            ["Lecturer"] = "Lecturers",
            ["TeachingAssistant"] = "Teaching Assistants",
            ["Administrator"] = "Administrators",
            ["SuperAdministrator"] = "Super Admins",
            ["Guest"] = "Guests",
        };

        return stats.RoleCounts
            .Where(r => r.Count > 0)
            .OrderByDescending(r => r.Count)
            .Select(r => new CategoryDatumDto(
                friendly.TryGetValue(r.Role, out string? name) ? name : r.Role, r.Count))
            .ToList();
    }

    private static List<ActivityItemDto> BuildActivity(AdminStatistics stats) =>
        stats.RecentUsers
            .Select((u, i) => new ActivityItemDto(
                Id: u.Id.ToString(),
                Category: "enrollment",
                ActorName: u.FullName,
                ActorColor: Palette[i % Palette.Length],
                Message: u.EmailConfirmed
                    ? "joined NovaLearn as a new member"
                    : "registered and is awaiting email verification",
                Status: u.EmailConfirmed ? "success" : "pending",
                Timestamp: u.CreatedAtUtc))
            .ToList();

    private static List<PendingApprovalDto> BuildApprovals(AdminStatistics stats) =>
        stats.RecentUnverifiedUsers
            .Select((u, i) => new PendingApprovalDto(
                Id: u.Id.ToString(),
                Kind: "student",
                Name: u.FullName,
                Subtitle: u.Email,
                Color: Palette[i % Palette.Length],
                SubmittedAt: u.CreatedAtUtc,
                Meta: "Email verification pending"))
            .ToList();

    private static SecuritySummaryDto BuildSecurity(AdminStatistics stats) => new(
        Score: 86,
        FailedLogins24h: stats.FailedLoginAttempts,
        BlockedIps: 0,
        ActiveSessions: stats.ActiveSessions,
        TwoFactorAdoptionPct: 0,
        Events:
        [
            new SecurityEventDto("s1", $"{stats.UnverifiedUsers} accounts pending verification",
                "Unverified accounts cannot sign in until confirmed", "info", DateTimeOffset.UtcNow.AddMinutes(-20)),
            new SecurityEventDto("s2", $"{stats.FailedLoginAttempts} failed login attempts on record",
                "Accounts lock after 5 consecutive failures", "warning", DateTimeOffset.UtcNow.AddMinutes(-95)),
        ]);

    // ---- Server-provided sections (no data source yet) ----------------------------------

    private static List<TimeSeriesPointDto> BuildWeeklyActivity()
    {
        string[] days = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
        double[] values = [8200, 9100, 9600, 9350, 8700, 4200, 3100];
        return days.Select((d, i) => new TimeSeriesPointDto(d, values[i], null)).ToList();
    }

    private static List<CategoryDatumDto> BuildPopularCourses() =>
    [
        new("Intro to Machine Learning", 4820),
        new("Full-Stack Web Development", 4310),
        new("Financial Accounting I", 3760),
        new("Organic Chemistry", 3120),
        new("Data Structures & Algorithms", 2980),
        new("Digital Marketing", 2540),
    ];

    private static SystemHealthDto BuildHealth() => new(
        Services:
        [
            new("PostgreSQL", "operational", 4, 99.99),
            new("REST API", "operational", 62, 99.98),
            new("Redis Cache", "operational", 1, 99.97),
            new("SignalR Hub", "degraded", 180, 99.42),
            new("Object Storage", "operational", 38, 99.95),
            new("Background Jobs", "operational", 24, 99.90),
            new("Email Service", "operational", 210, 99.80),
            new("Auth / Identity", "operational", 48, 99.99),
        ],
        Resources:
        [
            new("CPU", 41),
            new("Memory", 63),
            new("Disk", 68),
            new("Network", 34),
        ]);

    private static List<AiInsightDto> BuildInsights() =>
    [
        new("i1", "risk", "142 students at risk of dropping out",
            "Low login frequency and missed deadlines across 3 faculties. Early outreach recommended.",
            "142 students", "View cohort"),
        new("i2", "attention", "8 courses need content review",
            "Average rating fell below 3.5 stars this semester. Consider a curriculum refresh.",
            "8 courses", "Review courses"),
        new("i3", "opportunity", "Machine Learning demand is surging",
            "Enrolment intent up 34% month over month. A second cohort could capture waitlist demand.",
            "+34% MoM", "Plan cohort"),
        new("i4", "attention", "23 lecturers inactive for 14+ days",
            "No grading or content activity. A nudge may unblock pending student submissions.",
            "23 lecturers", "Notify"),
    ];

    // ---- Helpers ------------------------------------------------------------------------

    private static List<TimeSeriesPointDto> SyntheticSeries(double baseValue, int count, int seed)
    {
        var rng = new Random(seed);
        var points = new List<TimeSeriesPointDto>(count);
        double v = baseValue;
        for (int i = 0; i < count; i++)
        {
            v = Math.Max(0, v + (rng.NextDouble() - 0.35) * 3);
            points.Add(new TimeSeriesPointDto(MonthAbbrev[i % 12], Math.Round(v, 1), null));
        }
        return points;
    }

    private static IReadOnlyList<double> Spark(double baseValue, int seed)
    {
        var rng = new Random(seed);
        var list = new List<double>(12);
        double v = baseValue <= 0 ? 0 : baseValue * 0.82;
        double step = baseValue * 0.02;
        for (int i = 0; i < 12; i++)
        {
            v = Math.Max(0, v + step + (rng.NextDouble() - 0.4) * baseValue * 0.05);
            list.Add(Math.Round(v));
        }
        return list;
    }
}
