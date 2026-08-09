import { apiClient } from "@/services/apiClient";
import type { Department, DepartmentInput } from "./types";

export const departmentsApi = {
  async list(): Promise<Department[]> {
    const { data } = await apiClient.get<Department[]>("/admin/departments");
    return data;
  },

  async create(input: DepartmentInput): Promise<Department> {
    const { data } = await apiClient.post<Department>("/admin/departments", input);
    return data;
  },

  async update(id: string, input: DepartmentInput): Promise<Department> {
    const { data } = await apiClient.put<Department>(`/admin/departments/${id}`, input);
    return data;
  },

  async remove(id: string): Promise<void> {
    await apiClient.delete(`/admin/departments/${id}`);
  },
};
