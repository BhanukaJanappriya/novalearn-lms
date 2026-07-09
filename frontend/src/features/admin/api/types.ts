/**
 * Admin API contracts.
 *
 * These types are the stable seam between the UI and the data source. Today they
 * are served by a mock (`adminApi.ts`); swapping in real `.NET` endpoints means
 * changing only that file — every component below consumes these shapes unchanged.
 */

export type Trend = "up" | "down" | "flat";

/** Lucide icon identifiers the KPI/registry maps to real components. */
export type IconKey =
  | "users"
  | "graduation-cap"
  | "book-open"
  | "file-pen"
  | "clipboard-check"
  | "radio"
  | "user-plus"
  | "user-check"
  | "award"
  | "trending-up"
  | "dollar-sign"
  | "hard-drive"
  | "activity"
  | "database"
  | "server"
  | "message-square"
  | "life-buoy"
  | "calendar-check";

export type KpiFormat = "number" | "percent" | "currency" | "bytes";

export interface KpiMetric {
  id: string;
  label: string;
  value: number;
  format: KpiFormat;
  /** Percentage change vs the previous period (already computed server-side). */
  deltaPct: number;
  trend: Trend;
  /** Whether an upward movement is good (green) — dropout↑ is bad, for example. */
  higherIsBetter: boolean;
  icon: IconKey;
  /** Small gradient accent pair [from, to] as CSS colors. */
  accent: [string, string];
  /** ~12 points for the mini sparkline. */
  spark: number[];
  /** Optional deep-link target within the admin app. */
  href?: string;
  hint?: string;
}

export interface TimeSeriesPoint {
  /** Short axis label, e.g. "Jan" or "W23". */
  label: string;
  value: number;
  /** Optional secondary series on the SAME scale (never a second y-axis). */
  compare?: number;
}

export interface CategoryDatum {
  label: string;
  value: number;
}

export interface ExecutiveSummary {
  semesterName: string;
  semesterProgressPct: number;
  semesterStart: string;
  semesterEnd: string;
  activeUsersNow: number;
  serverHealthPct: number;
  pendingApprovals: number;
  systemStatus: "operational" | "degraded" | "down";
  academicPeriod: string;
}

export type ActivityCategory =
  | "enrollment"
  | "course"
  | "assignment"
  | "certificate"
  | "payment"
  | "support"
  | "announcement"
  | "forum";

export type ActivityStatus = "success" | "pending" | "info" | "warning";

export interface ActivityItem {
  id: string;
  category: ActivityCategory;
  actorName: string;
  actorColor: string;
  message: string;
  status: ActivityStatus;
  /** ISO timestamp. */
  timestamp: string;
}

export type ApprovalKind = "student" | "lecturer" | "course" | "certificate";

export interface PendingApproval {
  id: string;
  kind: ApprovalKind;
  name: string;
  subtitle: string;
  color: string;
  submittedAt: string;
  meta: string;
}

export type ServiceStatus = "operational" | "degraded" | "down";

export interface SystemService {
  name: string;
  status: ServiceStatus;
  latencyMs: number;
  uptimePct: number;
}

export interface SystemResource {
  name: string;
  usagePct: number;
}

export interface SystemHealth {
  services: SystemService[];
  resources: SystemResource[];
}

export interface SecurityEvent {
  id: string;
  label: string;
  detail: string;
  severity: "info" | "warning" | "critical";
  timestamp: string;
}

export interface SecuritySummary {
  score: number;
  failedLogins24h: number;
  blockedIps: number;
  activeSessions: number;
  twoFactorAdoptionPct: number;
  events: SecurityEvent[];
}

export type InsightSeverity = "opportunity" | "attention" | "risk";

export interface AiInsight {
  id: string;
  severity: InsightSeverity;
  title: string;
  body: string;
  /** A headline stat that anchors the insight, e.g. "142 students". */
  metric?: string;
  actionLabel: string;
}

/** The full dashboard payload. In a real API this maps to one aggregate endpoint. */
export interface AdminDashboard {
  summary: ExecutiveSummary;
  kpis: KpiMetric[];
  enrollmentTrend: TimeSeriesPoint[];
  completionTrend: TimeSeriesPoint[];
  roleDistribution: CategoryDatum[];
  weeklyActivity: TimeSeriesPoint[];
  popularCourses: CategoryDatum[];
  activity: ActivityItem[];
  approvals: PendingApproval[];
  health: SystemHealth;
  security: SecuritySummary;
  insights: AiInsight[];
}
