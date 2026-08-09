import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { departmentsApi } from "./departmentsApi";
import type { DepartmentInput } from "./types";

export const departmentKeys = {
  all: ["departments"] as const,
  list: () => [...departmentKeys.all, "list"] as const,
};

/**
 * Departments change rarely, so this is cached generously. Lecturers use it for the course
 * form's picker as well as admins for the management table.
 */
export function useDepartments(enabled = true) {
  return useQuery({
    queryKey: departmentKeys.list(),
    queryFn: () => departmentsApi.list(),
    enabled,
    staleTime: 5 * 60_000,
  });
}

export function useSaveDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, input }: { id?: string; input: DepartmentInput }) =>
      id ? departmentsApi.update(id, input) : departmentsApi.create(input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: departmentKeys.all }),
  });
}

export function useDeleteDepartment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => departmentsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: departmentKeys.all }),
  });
}
