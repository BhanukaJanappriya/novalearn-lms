import { apiClient } from "@/services/apiClient";
import type { CourseContent, CourseModule, Lesson, LessonPayload, ModulePayload } from "./types";

export const contentApi = {
  async get(courseId: string): Promise<CourseContent> {
    const { data } = await apiClient.get<CourseContent>(`/courses/${courseId}/content`);
    return data;
  },

  async createModule(courseId: string, payload: ModulePayload): Promise<CourseModule> {
    const { data } = await apiClient.post<CourseModule>(`/courses/${courseId}/modules`, payload);
    return data;
  },

  async updateModule(moduleId: string, payload: ModulePayload): Promise<CourseModule> {
    const { data } = await apiClient.put<CourseModule>(`/modules/${moduleId}`, payload);
    return data;
  },

  async removeModule(moduleId: string): Promise<void> {
    await apiClient.delete(`/modules/${moduleId}`);
  },

  async reorderModules(courseId: string, ids: string[]): Promise<void> {
    await apiClient.put(`/courses/${courseId}/modules/order`, { ids });
  },

  async createLesson(moduleId: string, payload: LessonPayload): Promise<Lesson> {
    const { data } = await apiClient.post<Lesson>(`/modules/${moduleId}/lessons`, payload);
    return data;
  },

  async updateLesson(lessonId: string, payload: LessonPayload): Promise<Lesson> {
    const { data } = await apiClient.put<Lesson>(`/lessons/${lessonId}`, payload);
    return data;
  },

  async removeLesson(lessonId: string): Promise<void> {
    await apiClient.delete(`/lessons/${lessonId}`);
  },

  async reorderLessons(moduleId: string, ids: string[]): Promise<void> {
    await apiClient.put(`/modules/${moduleId}/lessons/order`, { ids });
  },
};
