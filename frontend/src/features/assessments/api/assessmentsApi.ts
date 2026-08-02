import { apiClient } from "@/services/apiClient";
import type { Assignment, AssignmentInput, Gradebook, Submission } from "./types";

export const assessmentsApi = {
  async listAssignments(courseId: string): Promise<Assignment[]> {
    const { data } = await apiClient.get<Assignment[]>(`/courses/${courseId}/assignments`);
    return data;
  },

  async createAssignment(courseId: string, input: AssignmentInput): Promise<Assignment> {
    const { data } = await apiClient.post<Assignment>(`/courses/${courseId}/assignments`, input);
    return data;
  },

  async updateAssignment(assignmentId: string, input: AssignmentInput): Promise<Assignment> {
    const { data } = await apiClient.put<Assignment>(`/assignments/${assignmentId}`, input);
    return data;
  },

  async deleteAssignment(assignmentId: string): Promise<void> {
    await apiClient.delete(`/assignments/${assignmentId}`);
  },

  async submit(assignmentId: string, content: string, attachmentUrl: string | null): Promise<Submission> {
    const { data } = await apiClient.post<Submission>(`/assignments/${assignmentId}/submissions`, {
      content,
      attachmentUrl,
    });
    return data;
  },

  async listSubmissions(assignmentId: string): Promise<Submission[]> {
    const { data } = await apiClient.get<Submission[]>(`/assignments/${assignmentId}/submissions`);
    return data;
  },

  async grade(submissionId: string, pointsAwarded: number, feedback: string | null): Promise<Submission> {
    const { data } = await apiClient.put<Submission>(`/submissions/${submissionId}/grade`, {
      pointsAwarded,
      feedback,
    });
    return data;
  },

  async gradebook(courseId: string): Promise<Gradebook> {
    const { data } = await apiClient.get<Gradebook>(`/courses/${courseId}/gradebook`);
    return data;
  },
};
