import type {
  AdminDashboard,
  ActivityItem,
  CategoryDatum,
  KpiMetric,
  PendingApproval,
  TimeSeriesPoint,
} from "./types";

/**
 * Deterministic pseudo-random generator so the dashboard shows the SAME realistic
 * numbers on every render/refresh (a live API would return real figures here).
 */
function seeded(seed: number): () => number {
  let s = seed % 2147483647;
  if (s <= 0) s += 2147483646;
  return () => {
    s = (s * 16807) % 2147483647;
    return (s - 1) / 2147483646;
  };
}

const rand = seeded(20260709);

function jitterSeries(base: number, count: number, drift: number, noise: number): number[] {
  const out: number[] = [];
  let v = base;
  for (let i = 0; i < count; i++) {
    v = Math.max(0, v + drift + (rand() - 0.5) * noise);
    out.push(Math.round(v));
  }
  return out;
}

const MONTHS = ["Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan"];

function monthly(values: number[]): TimeSeriesPoint[] {
  return values.map((value, i) => ({ label: MONTHS[i % 12], value }));
}

const kpis: KpiMetric[] = [
  {
    id: "total-students",
    label: "Total Students",
    value: 48231,
    format: "number",
    deltaPct: 6.4,
    trend: "up",
    higherIsBetter: true,
    icon: "graduation-cap",
    accent: ["#8B5CF6", "#A78BFA"],
    spark: jitterSeries(44000, 12, 380, 600),
    href: "/admin/students",
    hint: "Enrolled learners across all faculties",
  },
  {
    id: "total-lecturers",
    label: "Total Lecturers",
    value: 1284,
    format: "number",
    deltaPct: 2.1,
    trend: "up",
    higherIsBetter: true,
    icon: "users",
    accent: ["#2a78d6", "#5598e7"],
    spark: jitterSeries(1200, 12, 8, 30),
    href: "/admin/lecturers",
  },
  {
    id: "courses-published",
    label: "Courses Published",
    value: 3126,
    format: "number",
    deltaPct: 4.8,
    trend: "up",
    higherIsBetter: true,
    icon: "book-open",
    accent: ["#1baf7a", "#3fd39c"],
    spark: jitterSeries(2900, 12, 20, 40),
    href: "/admin/courses",
  },
  {
    id: "courses-draft",
    label: "Courses in Draft",
    value: 214,
    format: "number",
    deltaPct: -3.2,
    trend: "down",
    higherIsBetter: false,
    icon: "file-pen",
    accent: ["#eda100", "#f5c451"],
    spark: jitterSeries(240, 12, -2, 20),
    href: "/admin/courses",
  },
  {
    id: "assignments-today",
    label: "Assignments Submitted",
    value: 6742,
    format: "number",
    deltaPct: 12.5,
    trend: "up",
    higherIsBetter: true,
    icon: "clipboard-check",
    accent: ["#4a3aa7", "#7a6ad4"],
    spark: jitterSeries(5200, 12, 120, 400),
    hint: "Today",
  },
  {
    id: "pending-reviews",
    label: "Pending Reviews",
    value: 389,
    format: "number",
    deltaPct: 8.9,
    trend: "up",
    higherIsBetter: false,
    icon: "clipboard-check",
    accent: ["#e34948", "#f07a79"],
    spark: jitterSeries(320, 12, 6, 40),
  },
  {
    id: "live-sessions",
    label: "Live Sessions",
    value: 47,
    format: "number",
    deltaPct: 15.0,
    trend: "up",
    higherIsBetter: true,
    icon: "radio",
    accent: ["#e34948", "#f07a79"],
    spark: jitterSeries(30, 12, 1.4, 12),
    hint: "In progress right now",
  },
  {
    id: "new-registrations",
    label: "New Registrations",
    value: 1893,
    format: "number",
    deltaPct: 9.7,
    trend: "up",
    higherIsBetter: true,
    icon: "user-plus",
    accent: ["#8B5CF6", "#c4b5fd"],
    spark: jitterSeries(1400, 12, 45, 200),
    hint: "Last 30 days",
  },
  {
    id: "certificates-issued",
    label: "Certificates Issued",
    value: 12905,
    format: "number",
    deltaPct: 5.3,
    trend: "up",
    higherIsBetter: true,
    icon: "award",
    accent: ["#eda100", "#f5c451"],
    spark: jitterSeries(11800, 12, 90, 220),
  },
  {
    id: "completion-rate",
    label: "Avg. Completion",
    value: 78.4,
    format: "percent",
    deltaPct: 1.9,
    trend: "up",
    higherIsBetter: true,
    icon: "trending-up",
    accent: ["#1baf7a", "#3fd39c"],
    spark: jitterSeries(74, 12, 0.4, 3),
  },
  {
    id: "revenue",
    label: "Revenue (MTD)",
    value: 486200,
    format: "currency",
    deltaPct: 11.2,
    trend: "up",
    higherIsBetter: true,
    icon: "dollar-sign",
    accent: ["#008300", "#3fb24a"],
    spark: jitterSeries(410000, 12, 6000, 24000),
    href: "/admin/finance",
  },
  {
    id: "storage",
    label: "Storage Used",
    value: 68.2,
    format: "percent",
    deltaPct: 3.4,
    trend: "up",
    higherIsBetter: false,
    icon: "hard-drive",
    accent: ["#2a78d6", "#5598e7"],
    spark: jitterSeries(60, 12, 0.7, 2),
  },
  {
    id: "mau",
    label: "Monthly Active Users",
    value: 39418,
    format: "number",
    deltaPct: 7.1,
    trend: "up",
    higherIsBetter: true,
    icon: "activity",
    accent: ["#4a3aa7", "#7a6ad4"],
    spark: jitterSeries(35000, 12, 380, 800),
  },
  {
    id: "attendance-rate",
    label: "Attendance Rate",
    value: 91.3,
    format: "percent",
    deltaPct: 0.8,
    trend: "up",
    higherIsBetter: true,
    icon: "calendar-check",
    accent: ["#1baf7a", "#3fd39c"],
    spark: jitterSeries(89, 12, 0.2, 2),
  },
  {
    id: "dropout-rate",
    label: "Dropout Rate",
    value: 4.6,
    format: "percent",
    deltaPct: -0.6,
    trend: "down",
    higherIsBetter: false,
    icon: "trending-up",
    accent: ["#e34948", "#f07a79"],
    spark: jitterSeries(6, 12, -0.1, 0.8),
  },
  {
    id: "support-tickets",
    label: "Open Support Tickets",
    value: 73,
    format: "number",
    deltaPct: -12.0,
    trend: "down",
    higherIsBetter: false,
    icon: "life-buoy",
    accent: ["#eda100", "#f5c451"],
    spark: jitterSeries(95, 12, -1.6, 14),
    href: "/admin/support",
  },
];

const enrollmentTrend = monthly(jitterSeries(2600, 12, 60, 260)).map((p, i) => ({
  ...p,
  compare: Math.round(p.value * (0.82 + i * 0.006)),
}));

const completionTrend = monthly(jitterSeries(70, 12, 0.7, 3));

const roleDistribution: CategoryDatum[] = [
  { label: "Students", value: 48231 },
  { label: "Lecturers", value: 1284 },
  { label: "Teaching Assistants", value: 612 },
  { label: "Administrators", value: 58 },
  { label: "Guests", value: 940 },
];

const weeklyActivity: TimeSeriesPoint[] = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map(
  (label, i) => ({ label, value: [8200, 9100, 9600, 9350, 8700, 4200, 3100][i] }),
);

const popularCourses: CategoryDatum[] = [
  { label: "Intro to Machine Learning", value: 4820 },
  { label: "Full-Stack Web Development", value: 4310 },
  { label: "Financial Accounting I", value: 3760 },
  { label: "Organic Chemistry", value: 3120 },
  { label: "Data Structures & Algorithms", value: 2980 },
  { label: "Digital Marketing", value: 2540 },
];

const activity: ActivityItem[] = [
  { id: "a1", category: "enrollment", actorName: "Amara Silva", actorColor: "#8B5CF6", message: "registered as a new student in Computer Science", status: "success", timestamp: iso(-3) },
  { id: "a2", category: "course", actorName: "Dr. Nuwan Perera", actorColor: "#2a78d6", message: "published “Advanced Databases (CS402)”", status: "success", timestamp: iso(-14) },
  { id: "a3", category: "assignment", actorName: "Kavya Fernando", actorColor: "#1baf7a", message: "submitted Assignment 3 for Data Structures", status: "info", timestamp: iso(-27) },
  { id: "a4", category: "certificate", actorName: "Ishara Bandara", actorColor: "#eda100", message: "was issued a certificate for Digital Marketing", status: "success", timestamp: iso(-52) },
  { id: "a5", category: "payment", actorName: "Ravindu Jayasuriya", actorColor: "#008300", message: "completed a tuition payment of $1,250", status: "success", timestamp: iso(-71) },
  { id: "a6", category: "support", actorName: "Sanduni Rathnayake", actorColor: "#e34948", message: "opened a support ticket about login issues", status: "warning", timestamp: iso(-96) },
  { id: "a7", category: "announcement", actorName: "Registrar Office", actorColor: "#4a3aa7", message: "posted “Mid-semester exam schedule released”", status: "info", timestamp: iso(-140) },
  { id: "a8", category: "forum", actorName: "Tharindu Alwis", actorColor: "#2a78d6", message: "replied in the Machine Learning discussion forum", status: "info", timestamp: iso(-185) },
];

const approvals: PendingApproval[] = [
  { id: "p1", kind: "lecturer", name: "Dr. Malini Gunawardena", subtitle: "Applied for Lecturer · Mathematics", color: "#2a78d6", submittedAt: iso(-40), meta: "PhD verified" },
  { id: "p2", kind: "student", name: "Dinuka Wijesinghe", subtitle: "Late enrolment · Software Engineering", color: "#8B5CF6", submittedAt: iso(-120), meta: "Documents attached" },
  { id: "p3", kind: "course", name: "Cloud Native Architecture", subtitle: "Submitted by Dr. R. Peris", color: "#1baf7a", submittedAt: iso(-220), meta: "12 modules" },
  { id: "p4", kind: "certificate", name: "Nethmi Karunaratne", subtitle: "Completion · UX Design Fundamentals", color: "#eda100", submittedAt: iso(-310), meta: "Score 94%" },
  { id: "p5", kind: "lecturer", name: "Prof. Asela Fonseka", subtitle: "Applied for Lecturer · Physics", color: "#4a3aa7", submittedAt: iso(-480), meta: "3 references" },
];

function iso(minutesAgo: number): string {
  return new Date(Date.now() + minutesAgo * 60_000).toISOString();
}

export const mockDashboard: AdminDashboard = {
  summary: {
    semesterName: "Fall 2026",
    semesterProgressPct: 62,
    semesterStart: "2026-08-25",
    semesterEnd: "2026-12-18",
    activeUsersNow: 3182,
    serverHealthPct: 99.98,
    pendingApprovals: approvals.length,
    systemStatus: "operational",
    academicPeriod: "Week 9 of 16 · Fall 2026",
  },
  kpis,
  enrollmentTrend,
  completionTrend,
  roleDistribution,
  weeklyActivity,
  popularCourses,
  activity,
  approvals,
  health: {
    services: [
      { name: "PostgreSQL", status: "operational", latencyMs: 4, uptimePct: 99.99 },
      { name: "REST API", status: "operational", latencyMs: 62, uptimePct: 99.98 },
      { name: "Redis Cache", status: "operational", latencyMs: 1, uptimePct: 99.97 },
      { name: "SignalR Hub", status: "degraded", latencyMs: 180, uptimePct: 99.42 },
      { name: "Object Storage", status: "operational", latencyMs: 38, uptimePct: 99.95 },
      { name: "Background Jobs", status: "operational", latencyMs: 24, uptimePct: 99.90 },
      { name: "Email Service", status: "operational", latencyMs: 210, uptimePct: 99.80 },
      { name: "Auth / Identity", status: "operational", latencyMs: 48, uptimePct: 99.99 },
    ],
    resources: [
      { name: "CPU", usagePct: 41 },
      { name: "Memory", usagePct: 63 },
      { name: "Disk", usagePct: 68 },
      { name: "Network", usagePct: 34 },
    ],
  },
  security: {
    score: 86,
    failedLogins24h: 128,
    blockedIps: 14,
    activeSessions: 3182,
    twoFactorAdoptionPct: 71,
    events: [
      { id: "s1", label: "Unusual login location", detail: "admin@novalearn.local from a new device (Colombo)", severity: "warning", timestamp: iso(-18) },
      { id: "s2", label: "5 failed login attempts", detail: "IP 203.0.113.47 temporarily rate-limited", severity: "critical", timestamp: iso(-64) },
      { id: "s3", label: "Role changed", detail: "Kavya F. promoted to Teaching Assistant", severity: "info", timestamp: iso(-150) },
      { id: "s4", label: "Password reset", detail: "12 self-service resets in the last hour", severity: "info", timestamp: iso(-200) },
    ],
  },
  insights: [
    { id: "i1", severity: "risk", title: "142 students at risk of dropping out", body: "Low login frequency + missed deadlines across 3 faculties. Early outreach recommended.", metric: "142 students", actionLabel: "View cohort" },
    { id: "i2", severity: "attention", title: "8 courses need content review", body: "Average rating fell below 3.5★ this semester. Consider a curriculum refresh.", metric: "8 courses", actionLabel: "Review courses" },
    { id: "i3", severity: "opportunity", title: "Machine Learning demand is surging", body: "Enrolment intent up 34% MoM. Adding a second cohort could capture waitlist demand.", metric: "+34% MoM", actionLabel: "Plan cohort" },
    { id: "i4", severity: "attention", title: "23 lecturers inactive for 14+ days", body: "No grading or content activity. A nudge may unblock pending student submissions.", metric: "23 lecturers", actionLabel: "Notify" },
  ],
};
