import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";

export type AnalyticsGranularity = "Day" | "Week" | "Month";

/**
 * A figure alongside the same figure for the window before it.
 *
 * `changePercent` is null when the previous window was empty, which the page renders as "no
 * comparison" rather than as an enormous rise.
 */
export interface TrendMetric {
  current: number;
  previous: number;
  changePercent: number | null;
}

export interface AnalyticsWindow {
  fromUtc: string;
  toUtc: string;
  days: number;
  granularity: AnalyticsGranularity;
}

export interface AnalyticsHeadline {
  enrolments: TrendMetric;
  completions: TrendMetric;
  activeLearners: TrendMetric;
  averageScorePercent: TrendMetric;
}

export interface AnalyticsPoint {
  date: string;
  enrolments: number;
  completions: number;
}

export interface CoursePerformanceRow {
  courseId: string;
  title: string;
  departmentId: string | null;
  departmentName: string | null;
  enrolled: number;
  completed: number;
  completionRatePercent: number;
  averageProgressPercent: number;
  averageScorePercent: number | null;
  atRiskCount: number;
}

export interface DepartmentPerformanceRow {
  departmentId: string | null;
  name: string;
  courses: number;
  enrolled: number;
  completed: number;
  completionRatePercent: number;
}

export interface ScoreBucket {
  label: string;
  count: number;
}

export interface PlatformAnalytics {
  window: AnalyticsWindow;
  headline: AnalyticsHeadline;
  series: AnalyticsPoint[];
  courses: CoursePerformanceRow[];
  departments: DepartmentPerformanceRow[];
  scoreDistribution: ScoreBucket[];
}

export const analyticsKeys = {
  all: ["analytics"] as const,
  window: (days: number) => [...analyticsKeys.all, days] as const,
};

export function useAnalytics(days: number) {
  return useQuery({
    queryKey: analyticsKeys.window(days),
    queryFn: async () => {
      const { data } = await apiClient.get<PlatformAnalytics>("/admin/analytics", {
        params: { days },
      });
      return data;
    },
    staleTime: 60_000,
  });
}
