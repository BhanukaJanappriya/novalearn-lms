import { Badge } from "@/components/ui/badge";
import type { AdminUser } from "@/features/users/api/types";
import { formatMoney, statusLabel as paymentStatusLabel, statusVariant as paymentStatusVariant } from "@/features/finance/lib/finance";
import {
  categoryLabel,
  priorityVariant,
  statusLabel as ticketStatusLabel,
  statusVariant as ticketStatusVariant,
} from "@/features/support/lib/support";
import type { DisplayColumn } from "../components/ReportTable";
import type {
  CoursePerformanceRow,
  EnrollmentReportRow,
  EnrollmentStatus,
  TicketSummary,
  Transaction,
} from "../api/types";

type BadgeVariant = "default" | "neutral" | "success" | "warning" | "destructive" | "outline";

const enrollmentStatusVariant: Record<EnrollmentStatus, BadgeVariant> = {
  Active: "default",
  Completed: "success",
  Dropped: "neutral",
};

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

export const enrollmentColumns: DisplayColumn<EnrollmentReportRow>[] = [
  { header: "Student", get: (row) => row.studentName },
  { header: "Email", get: (row) => row.studentEmail },
  { header: "Course", get: (row) => row.courseTitle },
  {
    header: "Status",
    get: (row) => row.status,
    render: (row) => <Badge variant={enrollmentStatusVariant[row.status]}>{row.status}</Badge>,
  },
  { header: "Progress", get: (row) => `${row.progressPercent}%`, align: "right" },
  { header: "Enrolled", get: (row) => formatDate(row.enrolledAtUtc) },
  { header: "Completed", get: (row) => (row.completedAtUtc ? formatDate(row.completedAtUtc) : "—") },
];

export const revenueColumns: DisplayColumn<Transaction>[] = [
  { header: "Course", get: (row) => row.courseTitle },
  { header: "Student", get: (row) => row.studentName },
  { header: "Amount", get: (row) => formatMoney(row.amount, row.currency), align: "right" },
  {
    header: "Status",
    get: (row) => paymentStatusLabel[row.status],
    render: (row) => <Badge variant={paymentStatusVariant[row.status]}>{paymentStatusLabel[row.status]}</Badge>,
  },
  { header: "Date", get: (row) => formatDate(row.createdAtUtc) },
];

export const coursePerformanceColumns: DisplayColumn<CoursePerformanceRow>[] = [
  { header: "Course", get: (row) => row.title },
  { header: "Department", get: (row) => row.departmentName ?? "No department" },
  { header: "Enrolled", get: (row) => row.enrolled, align: "right" },
  { header: "Completed", get: (row) => row.completed, align: "right" },
  { header: "Completion rate", get: (row) => `${row.completionRatePercent}%`, align: "right" },
  { header: "Avg. progress", get: (row) => `${row.averageProgressPercent}%`, align: "right" },
  {
    header: "Avg. score",
    get: (row) => (row.averageScorePercent === null ? "—" : `${row.averageScorePercent}%`),
    align: "right",
  },
  { header: "At risk", get: (row) => row.atRiskCount, align: "right" },
];

export const usersColumns: DisplayColumn<AdminUser>[] = [
  { header: "Name", get: (row) => row.fullName },
  { header: "Email", get: (row) => row.email },
  { header: "Roles", get: (row) => row.roles.join(", ") },
  {
    header: "Active",
    get: (row) => (row.isActive ? "Yes" : "No"),
    render: (row) => (
      <Badge variant={row.isActive ? "success" : "neutral"}>{row.isActive ? "Active" : "Inactive"}</Badge>
    ),
  },
  { header: "Verified", get: (row) => (row.emailConfirmed ? "Yes" : "No") },
  { header: "Enrollments", get: (row) => row.enrollmentCount, align: "right" },
  { header: "Joined", get: (row) => formatDate(row.createdAtUtc) },
];

export const supportTicketsColumns: DisplayColumn<TicketSummary>[] = [
  { header: "Subject", get: (row) => row.subject },
  { header: "Student", get: (row) => row.submittedByName },
  { header: "Category", get: (row) => categoryLabel[row.category] },
  {
    header: "Priority",
    get: (row) => row.priority,
    render: (row) => <Badge variant={priorityVariant[row.priority]}>{row.priority}</Badge>,
  },
  {
    header: "Status",
    get: (row) => ticketStatusLabel[row.status],
    render: (row) => <Badge variant={ticketStatusVariant[row.status]}>{ticketStatusLabel[row.status]}</Badge>,
  },
  { header: "Assigned to", get: (row) => row.assignedToName ?? "Unclaimed" },
  { header: "Created", get: (row) => formatDate(row.createdAtUtc) },
];
