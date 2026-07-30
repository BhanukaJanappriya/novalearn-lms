import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { contentApi } from "./contentApi";
import type { LessonPayload, ModulePayload } from "./types";

export const contentKeys = {
  all: ["course-content"] as const,
  detail: (courseId: string) => [...contentKeys.all, courseId] as const,
};

export function useCourseContent(courseId: string) {
  return useQuery({
    queryKey: contentKeys.detail(courseId),
    queryFn: () => contentApi.get(courseId),
    staleTime: 15_000,
    enabled: Boolean(courseId),
  });
}

/**
 * Every mutation below reshapes the tree that `useCourseContent` returns, so they all
 * invalidate the same course's detail key on success.
 */
function useContentMutation<TVariables, TData>(
  courseId: string,
  mutationFn: (variables: TVariables) => Promise<TData>,
) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: contentKeys.detail(courseId) }),
  });
}

export function useCreateModule(courseId: string) {
  return useContentMutation(courseId, (payload: ModulePayload) =>
    contentApi.createModule(courseId, payload),
  );
}

export function useUpdateModule(courseId: string) {
  return useContentMutation(courseId, ({ id, payload }: { id: string; payload: ModulePayload }) =>
    contentApi.updateModule(id, payload),
  );
}

export function useDeleteModule(courseId: string) {
  return useContentMutation(courseId, (moduleId: string) => contentApi.removeModule(moduleId));
}

export function useReorderModules(courseId: string) {
  return useContentMutation(courseId, (ids: string[]) => contentApi.reorderModules(courseId, ids));
}

export function useCreateLesson(courseId: string) {
  return useContentMutation(
    courseId,
    ({ moduleId, payload }: { moduleId: string; payload: LessonPayload }) =>
      contentApi.createLesson(moduleId, payload),
  );
}

export function useUpdateLesson(courseId: string) {
  return useContentMutation(courseId, ({ id, payload }: { id: string; payload: LessonPayload }) =>
    contentApi.updateLesson(id, payload),
  );
}

export function useDeleteLesson(courseId: string) {
  return useContentMutation(courseId, (lessonId: string) => contentApi.removeLesson(lessonId));
}

export function useReorderLessons(courseId: string) {
  return useContentMutation(courseId, ({ moduleId, ids }: { moduleId: string; ids: string[] }) =>
    contentApi.reorderLessons(moduleId, ids),
  );
}
