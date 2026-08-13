import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";
import { assessmentKeys } from "./queries";
import type { AssessmentStatus } from "./types";

export type AssessmentKind = "Assignment" | "Quiz";

/**
 * Mirrors the backend `AssessmentOverviewRow`. Counts only: who submitted what and who scored
 * what belongs to the per assignment and per quiz screens this page links to.
 */
export interface AssessmentOverviewItem {
  kind: AssessmentKind;
  id: string;
  courseId: string;
  courseTitle: string;
  title: string;
  status: AssessmentStatus;
  dueAtUtc: string | null;
  maxPoints: number;
  questionCount: number;
  enrolledCount: number;
  submittedCount: number;
  awaitingMarkingCount: number;
  gradedCount: number;
  averageScorePercent: number | null;
}

export interface AssessmentOverviewSummary {
  total: number;
  published: number;
  drafts: number;
  awaitingMarking: number;
  dueSoon: number;
  overdue: number;
}

export interface AssessmentOverview {
  summary: AssessmentOverviewSummary;
  items: AssessmentOverviewItem[];
}

export const overviewKey = [...assessmentKeys.all, "overview"] as const;

/**
 * Every assessment the signed-in member of staff is responsible for. The server decides the
 * scope from the token, so there is no course parameter to get wrong here.
 */
export function useAssessmentOverview() {
  return useQuery({
    queryKey: overviewKey,
    queryFn: async () => {
      const { data } = await apiClient.get<AssessmentOverview>("/assessments/overview");
      return data;
    },
    staleTime: 15_000,
  });
}
