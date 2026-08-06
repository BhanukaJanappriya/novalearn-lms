import { apiClient } from "@/services/apiClient";
import type {
  AttemptInProgress,
  AttemptResult,
  QuizAuthoring,
  QuizInput,
  QuizResults,
  QuizSummary,
  SaveQuestionInput,
} from "./types";

export const quizzesApi = {
  async list(courseId: string): Promise<QuizSummary[]> {
    const { data } = await apiClient.get<QuizSummary[]>(`/courses/${courseId}/quizzes`);
    return data;
  },

  async create(courseId: string, input: QuizInput): Promise<QuizSummary> {
    const { data } = await apiClient.post<QuizSummary>(`/courses/${courseId}/quizzes`, input);
    return data;
  },

  async update(quizId: string, input: QuizInput): Promise<QuizSummary> {
    const { data } = await apiClient.put<QuizSummary>(`/quizzes/${quizId}`, input);
    return data;
  },

  async remove(quizId: string): Promise<void> {
    await apiClient.delete(`/quizzes/${quizId}`);
  },

  /** Staff only: this is the payload that carries correct answers. */
  async authoring(quizId: string): Promise<QuizAuthoring> {
    const { data } = await apiClient.get<QuizAuthoring>(`/quizzes/${quizId}/authoring`);
    return data;
  },

  async saveQuestion(quizId: string, input: SaveQuestionInput): Promise<void> {
    await apiClient.put(`/quizzes/${quizId}/questions`, input);
  },

  async deleteQuestion(questionId: string): Promise<void> {
    await apiClient.delete(`/questions/${questionId}`);
  },

  async results(quizId: string): Promise<QuizResults> {
    const { data } = await apiClient.get<QuizResults>(`/quizzes/${quizId}/results`);
    return data;
  },

  async startAttempt(quizId: string): Promise<AttemptInProgress> {
    const { data } = await apiClient.post<AttemptInProgress>(`/quizzes/${quizId}/attempts`);
    return data;
  },

  async saveAnswer(
    attemptId: string,
    questionId: string,
    selectedOptionIds: string[],
    textAnswer: string | null,
  ): Promise<void> {
    await apiClient.put(`/attempts/${attemptId}/answers`, {
      questionId,
      selectedOptionIds,
      textAnswer,
    });
  },

  async reorderQuestions(quizId: string, questionIds: string[]): Promise<QuizAuthoring> {
    const { data } = await apiClient.put<QuizAuthoring>(`/quizzes/${quizId}/questions/order`, {
      questionIds,
    });
    return data;
  },

  async duplicateQuestion(questionId: string): Promise<void> {
    await apiClient.post(`/questions/${questionId}/duplicate`);
  },

  /** Records a person's mark on one essay answer. */
  async markEssay(
    attemptId: string,
    answerId: string,
    pointsAwarded: number,
    feedback: string | null,
  ): Promise<AttemptResult> {
    const { data } = await apiClient.put<AttemptResult>(
      `/attempts/${attemptId}/answers/${answerId}/mark`,
      { pointsAwarded, feedback },
    );
    return data;
  },

  async submitAttempt(attemptId: string): Promise<AttemptResult> {
    const { data } = await apiClient.post<AttemptResult>(`/attempts/${attemptId}/submit`);
    return data;
  },

  async attemptResult(attemptId: string): Promise<AttemptResult> {
    const { data } = await apiClient.get<AttemptResult>(`/attempts/${attemptId}`);
    return data;
  },
};
