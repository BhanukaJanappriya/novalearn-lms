import type { PaymentStatus, Transaction } from "@/features/finance/api/queries";
import type { CoursePerformanceRow } from "@/features/analytics/api/queries";
import type { TicketCategory, TicketPriority, TicketStatus, TicketSummary } from "@/features/support/api/types";

export type {
  PaymentStatus,
  Transaction,
  CoursePerformanceRow,
  TicketSummary,
  TicketCategory,
  TicketPriority,
  TicketStatus,
};

export type ReportType = "Enrollments" | "Revenue" | "CoursePerformance" | "Users" | "SupportTickets";

export type EnrollmentStatus = "Active" | "Completed" | "Dropped";

/** The one report row with no existing read model to reuse elsewhere in the app. */
export interface EnrollmentReportRow {
  enrollmentId: string;
  studentId: string;
  studentName: string;
  studentEmail: string;
  courseId: string;
  courseTitle: string;
  status: EnrollmentStatus;
  progressPercent: number;
  enrolledAtUtc: string;
  completedAtUtc: string | null;
}

/** One entry in the "recent report runs" audit panel. */
export interface ReportRun {
  id: string;
  type: ReportType;
  generatedByName: string;
  filtersSummary: string | null;
  rowCount: number;
  createdAtUtc: string;
}

export interface EnrollmentReportFilters {
  status?: EnrollmentStatus;
  fromUtc?: string;
  toUtc?: string;
}

export interface RevenueReportFilters {
  status?: PaymentStatus;
  fromUtc?: string;
  toUtc?: string;
}

export interface UsersReportFilters {
  search?: string;
  role?: string;
  isActive?: boolean;
  emailConfirmed?: boolean;
}

export interface SupportTicketsReportFilters {
  status?: TicketStatus;
  category?: TicketCategory;
  priority?: TicketPriority;
}
