import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { assessmentsApi } from "./assessmentsApi";
import type { AssignmentInput } from "./types";

export const assessmentKeys = {
  all: ["assessments"] as const,
  assignments: (courseId: string) => [...assessmentKeys.all, "assignments", courseId] as const,
  submissions: (assignmentId: string) => [...assessmentKeys.all, "submissions", assignmentId] as const,
  gradebook: (courseId: string) => [...assessmentKeys.all, "gradebook", courseId] as const,
};

export function useAssignments(courseId: string | undefined) {
  return useQuery({
    queryKey: assessmentKeys.assignments(courseId ?? ""),
    queryFn: () => assessmentsApi.listAssignments(courseId!),
    enabled: Boolean(courseId),
    staleTime: 15_000,
  });
}

export function useSubmissions(assignmentId: string | undefined) {
  return useQuery({
    queryKey: assessmentKeys.submissions(assignmentId ?? ""),
    queryFn: () => assessmentsApi.listSubmissions(assignmentId!),
    enabled: Boolean(assignmentId),
    staleTime: 10_000,
  });
}

export function useGradebook(courseId: string | undefined) {
  return useQuery({
    queryKey: assessmentKeys.gradebook(courseId ?? ""),
    queryFn: () => assessmentsApi.gradebook(courseId!),
    enabled: Boolean(courseId),
    staleTime: 15_000,
  });
}

export function useCreateAssignment(courseId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: AssignmentInput) => assessmentsApi.createAssignment(courseId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: assessmentKeys.all }),
  });
}

export function useUpdateAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ assignmentId, input }: { assignmentId: string; input: AssignmentInput }) =>
      assessmentsApi.updateAssignment(assignmentId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: assessmentKeys.all }),
  });
}

export function useDeleteAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (assignmentId: string) => assessmentsApi.deleteAssignment(assignmentId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: assessmentKeys.all }),
  });
}

export function useSubmitAssignment() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      assignmentId,
      content,
      attachmentUrl,
    }: {
      assignmentId: string;
      content: string;
      attachmentUrl: string | null;
    }) => assessmentsApi.submit(assignmentId, content, attachmentUrl),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: assessmentKeys.all }),
  });
}

export function useGradeSubmission() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      submissionId,
      pointsAwarded,
      feedback,
    }: {
      submissionId: string;
      pointsAwarded: number;
      feedback: string | null;
    }) => assessmentsApi.grade(submissionId, pointsAwarded, feedback),
    // Marking moves the gradebook and the assignment tallies as well as the roster.
    onSuccess: () => queryClient.invalidateQueries({ queryKey: assessmentKeys.all }),
  });
}
