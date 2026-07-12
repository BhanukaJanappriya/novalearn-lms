import { apiClient } from "@/services/apiClient";
import type { Course, CreateCoursePayload } from "./types";

export const coursesApi = {
  async list(): Promise<Course[]> {
    const { data } = await apiClient.get<Course[]>("/courses");
    return data;
  },

  async create(payload: CreateCoursePayload): Promise<Course> {
    const { data } = await apiClient.post<Course>("/courses", payload);
    return data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/courses/${id}`);
  },
};
