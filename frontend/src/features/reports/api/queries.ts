import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";
import type { AdminUser } from "@/features/users/api/types";
import type {
  CoursePerformanceRow,
  EnrollmentReportFilters,
  EnrollmentReportRow,
  ReportRun,
  RevenueReportFilters,
  SupportTicketsReportFilters,
  TicketSummary,
  Transaction,
  UsersReportFilters,
} from "./types";

export const reportsKeys = {
  all: ["reports"] as const,
  enrollments: (filters: EnrollmentReportFilters) => [...reportsKeys.all, "enrollments", filters] as const,
  revenue: (filters: RevenueReportFilters) => [...reportsKeys.all, "revenue", filters] as const,
  coursePerformance: [...reportsKeys.all, "course-performance"] as const,
  users: (filters: UsersReportFilters) => [...reportsKeys.all, "users", filters] as const,
  supportTickets: (filters: SupportTicketsReportFilters) =>
    [...reportsKeys.all, "support-tickets", filters] as const,
  recentRuns: [...reportsKeys.all, "recent-runs"] as const,
};

/**
 * Every report query is disabled by default: a report is something staff explicitly asks for (it
 * is logged on the server the moment it runs), not something that should fire on every filter
 * keystroke the way a live list does. The page enables a hook only long enough to call its
 * `refetch`, from the "Generate report" button.
 */
export function useEnrollmentsReport(filters: EnrollmentReportFilters, enabled: boolean) {
  return useQuery({
    queryKey: reportsKeys.enrollments(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<EnrollmentReportRow[]>("/admin/reports/enrollments", {
        params: filters,
      });
      return data;
    },
    enabled,
  });
}

export function useRevenueReport(filters: RevenueReportFilters, enabled: boolean) {
  return useQuery({
    queryKey: reportsKeys.revenue(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<Transaction[]>("/admin/reports/revenue", { params: filters });
      return data;
    },
    enabled,
  });
}

export function useCoursePerformanceReport(enabled: boolean) {
  return useQuery({
    queryKey: reportsKeys.coursePerformance,
    queryFn: async () => {
      const { data } = await apiClient.get<CoursePerformanceRow[]>("/admin/reports/course-performance");
      return data;
    },
    enabled,
  });
}

export function useUsersReport(filters: UsersReportFilters, enabled: boolean) {
  return useQuery({
    queryKey: reportsKeys.users(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<AdminUser[]>("/admin/reports/users", { params: filters });
      return data;
    },
    enabled,
  });
}

export function useSupportTicketsReport(filters: SupportTicketsReportFilters, enabled: boolean) {
  return useQuery({
    queryKey: reportsKeys.supportTickets(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<TicketSummary[]>("/admin/reports/support-tickets", {
        params: filters,
      });
      return data;
    },
    enabled,
  });
}

/** The "recent report runs" audit panel: who ran what, when. Loads on its own, unlike the reports. */
export function useRecentReportRuns() {
  return useQuery({
    queryKey: reportsKeys.recentRuns,
    queryFn: async () => {
      const { data } = await apiClient.get<ReportRun[]>("/admin/reports/recent-runs", {
        params: { count: 20 },
      });
      return data;
    },
    staleTime: 10_000,
  });
}
