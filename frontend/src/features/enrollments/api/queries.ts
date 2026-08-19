import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { studentKeys } from "@/features/student/api/queries";
import { enrollmentsApi } from "./enrollmentsApi";
import type { CatalogFilters } from "./types";

export const enrollmentKeys = {
  all: ["enrollments"] as const,
  mine: () => [...enrollmentKeys.all, "mine"] as const,
  roster: (courseId: string) => [...enrollmentKeys.all, "roster", courseId] as const,
  catalog: () => [...enrollmentKeys.all, "catalog"] as const,
  catalogPage: (filters: CatalogFilters) => [...enrollmentKeys.catalog(), filters] as const,
};

/** Paged, filtered catalogue. Previous data is kept so paging does not flash a skeleton. */
export function useCourseCatalog(filters: CatalogFilters) {
  return useQuery({
    queryKey: enrollmentKeys.catalogPage(filters),
    queryFn: () => enrollmentsApi.catalog(filters),
    placeholderData: keepPreviousData,
    staleTime: 15_000,
  });
}

export function useMyEnrollments(options?: { refetchInterval?: number | false }) {
  return useQuery({
    queryKey: enrollmentKeys.mine(),
    queryFn: () => enrollmentsApi.mine(),
    staleTime: 15_000,
    refetchInterval: options?.refetchInterval,
  });
}

export function useCourseRoster(courseId: string | undefined) {
  return useQuery({
    queryKey: enrollmentKeys.roster(courseId ?? ""),
    queryFn: () => enrollmentsApi.roster(courseId!),
    enabled: Boolean(courseId),
    staleTime: 15_000,
  });
}

export function useEnrollInCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (courseId: string) => enrollmentsApi.enroll(courseId),
    // Enrolling changes the catalogue's isEnrolled/enrolledCount and "my courses".
    onSuccess: () => queryClient.invalidateQueries({ queryKey: enrollmentKeys.all }),
  });
}

export function useUpdateProgress() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ enrollmentId, progressPercent }: { enrollmentId: string; progressPercent: number }) =>
      enrollmentsApi.updateProgress(enrollmentId, progressPercent),
    // Progress drives the dashboard summary as well as "my courses", so refresh both.
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: enrollmentKeys.all });
      void queryClient.invalidateQueries({ queryKey: studentKeys.all });
    },
  });
}

export function useUnenroll() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (enrollmentId: string) => enrollmentsApi.unenroll(enrollmentId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: enrollmentKeys.all }),
  });
}
