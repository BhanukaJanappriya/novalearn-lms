import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { quizzesApi } from "./quizzesApi";
import type { QuizInput, SaveQuestionInput } from "./types";

export const quizKeys = {
  all: ["quizzes"] as const,
  list: (courseId: string) => [...quizKeys.all, "list", courseId] as const,
  authoring: (quizId: string) => [...quizKeys.all, "authoring", quizId] as const,
  results: (quizId: string) => [...quizKeys.all, "results", quizId] as const,
  attempt: (attemptId: string) => [...quizKeys.all, "attempt", attemptId] as const,
};

export function useQuizzes(courseId: string | undefined) {
  return useQuery({
    queryKey: quizKeys.list(courseId ?? ""),
    queryFn: () => quizzesApi.list(courseId!),
    enabled: Boolean(courseId),
    staleTime: 15_000,
  });
}

/** Staff only. Fetching this as a learner returns 403 rather than a redacted quiz. */
export function useQuizAuthoring(quizId: string | undefined) {
  return useQuery({
    queryKey: quizKeys.authoring(quizId ?? ""),
    queryFn: () => quizzesApi.authoring(quizId!),
    enabled: Boolean(quizId),
    staleTime: 10_000,
  });
}

export function useQuizResults(quizId: string | undefined) {
  return useQuery({
    queryKey: quizKeys.results(quizId ?? ""),
    queryFn: () => quizzesApi.results(quizId!),
    enabled: Boolean(quizId),
    staleTime: 10_000,
  });
}

export function useAttemptResult(attemptId: string | undefined) {
  return useQuery({
    queryKey: quizKeys.attempt(attemptId ?? ""),
    queryFn: () => quizzesApi.attemptResult(attemptId!),
    enabled: Boolean(attemptId),
  });
}

export function useCreateQuiz(courseId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: QuizInput) => quizzesApi.create(courseId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: quizKeys.all }),
  });
}

export function useUpdateQuiz() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ quizId, input }: { quizId: string; input: QuizInput }) =>
      quizzesApi.update(quizId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: quizKeys.all }),
  });
}

export function useDeleteQuiz() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (quizId: string) => quizzesApi.remove(quizId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: quizKeys.all }),
  });
}

export function useSaveQuestion(quizId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: SaveQuestionInput) => quizzesApi.saveQuestion(quizId, input),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: quizKeys.all }),
  });
}

export function useDeleteQuestion() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (questionId: string) => quizzesApi.deleteQuestion(questionId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: quizKeys.all }),
  });
}

export function useStartAttempt() {
  return useMutation({
    mutationFn: (quizId: string) => quizzesApi.startAttempt(quizId),
  });
}

/**
 * Saves one answer. Deliberately does not invalidate anything: a refetch mid-quiz would
 * reorder a shuffled paper under the learner.
 */
export function useSaveAnswer() {
  return useMutation({
    mutationFn: ({
      attemptId,
      questionId,
      selectedOptionId,
      textAnswer,
    }: {
      attemptId: string;
      questionId: string;
      selectedOptionId: string | null;
      textAnswer: string | null;
    }) => quizzesApi.saveAnswer(attemptId, questionId, selectedOptionId, textAnswer),
  });
}

export function useSubmitAttempt() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (attemptId: string) => quizzesApi.submitAttempt(attemptId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: quizKeys.all }),
  });
}
