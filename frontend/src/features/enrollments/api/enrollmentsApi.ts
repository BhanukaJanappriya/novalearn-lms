import { apiClient } from "@/services/apiClient";
import type { CatalogCourse, CatalogFilters, Enrollment, PagedResult } from "./types";

export const enrollmentsApi = {
  async catalog(filters: CatalogFilters): Promise<PagedResult<CatalogCourse>> {
    const { data } = await apiClient.get<PagedResult<CatalogCourse>>("/courses/catalog", {
      // Blank filters are omitted so the server keeps its defaults.
      params: {
        search: filters.search || undefined,
        category: filters.category || undefined,
        level: filters.level || undefined,
        page: filters.page,
        pageSize: filters.pageSize,
      },
    });
    return data;
  },

  async enroll(courseId: string): Promise<Enrollment> {
    const { data } = await apiClient.post<Enrollment>(`/courses/${courseId}/enrollments`);
    return data;
  },

  async mine(): Promise<Enrollment[]> {
    const { data } = await apiClient.get<Enrollment[]>("/enrollments/me");
    return data;
  },

  async updateProgress(enrollmentId: string, progressPercent: number): Promise<Enrollment> {
    const { data } = await apiClient.put<Enrollment>(`/enrollments/${enrollmentId}/progress`, {
      progressPercent,
    });
    return data;
  },

  async unenroll(enrollmentId: string): Promise<void> {
    await apiClient.delete(`/enrollments/${enrollmentId}`);
  },

  async roster(courseId: string): Promise<Enrollment[]> {
    const { data } = await apiClient.get<Enrollment[]>(`/courses/${courseId}/enrollments`);
    return data;
  },
};
