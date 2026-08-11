import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/services/apiClient";

export interface DirectoryLearnerStats {
  enrolledCourses: number;
  completedCourses: number;
  averageProgressPercent: number;
}

export interface DirectoryTeacherStats {
  coursesOwned: number;
  publishedCourses: number;
  learnersTaught: number;
  departmentsHeaded: string[];
}

/**
 * Mirrors the backend `DirectoryEntryDto`. Deliberately carries no account-security state: the
 * directory answers who is here, and /admin/users is where accounts are acted on.
 */
export interface DirectoryEntry {
  id: string;
  fullName: string;
  firstName: string;
  lastName: string;
  email: string;
  avatarUrl: string | null;
  isActive: boolean;
  joinedAtUtc: string;
  lastActiveAtUtc: string | null;
  roles: string[];
  learner: DirectoryLearnerStats | null;
  teacher: DirectoryTeacherStats | null;
}

export type DirectoryAudience = "students" | "lecturers";

export const directoryKeys = {
  all: ["directory"] as const,
  list: (audience: DirectoryAudience, search: string) =>
    [...directoryKeys.all, audience, search] as const,
};

export function useDirectory(audience: DirectoryAudience, search: string) {
  return useQuery({
    queryKey: directoryKeys.list(audience, search),
    queryFn: async () => {
      const { data } = await apiClient.get<DirectoryEntry[]>(`/admin/directory/${audience}`, {
        params: { search: search || undefined },
      });
      return data;
    },
    staleTime: 30_000,
  });
}
