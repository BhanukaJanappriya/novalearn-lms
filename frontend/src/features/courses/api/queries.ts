import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { coursesApi } from "./coursesApi";
import type { CreateCoursePayload } from "./types";

export const courseKeys = {
  all: ["courses"] as const,
  list: () => [...courseKeys.all, "list"] as const,
};

export function useCourses() {
  return useQuery({
    queryKey: courseKeys.list(),
    queryFn: () => coursesApi.list(),
    staleTime: 15_000,
  });
}

export function useCreateCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateCoursePayload) => coursesApi.create(payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: courseKeys.all }),
  });
}

export function useDeleteCourse() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => coursesApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: courseKeys.all }),
  });
}
