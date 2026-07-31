import { useQuery } from "@tanstack/react-query";
import { studentApi } from "./studentApi";

export const studentKeys = {
  all: ["student"] as const,
  dashboard: () => [...studentKeys.all, "dashboard"] as const,
};

export function useStudentDashboard() {
  return useQuery({
    queryKey: studentKeys.dashboard(),
    queryFn: () => studentApi.dashboard(),
    staleTime: 15_000,
  });
}
