import { apiClient } from "@/services/apiClient";
import type { AdminUser, PagedResult, UserFilters } from "./types";

export const usersApi = {
  async list(filters: UserFilters): Promise<PagedResult<AdminUser>> {
    const { data } = await apiClient.get<PagedResult<AdminUser>>("/admin/users", {
      // Blank filters are omitted so the server keeps its defaults.
      params: {
        search: filters.search || undefined,
        role: filters.role || undefined,
        isActive: filters.isActive,
        emailConfirmed: filters.emailConfirmed,
        page: filters.page,
        pageSize: filters.pageSize,
      },
    });
    return data;
  },

  async roles(): Promise<string[]> {
    const { data } = await apiClient.get<string[]>("/admin/roles");
    return data;
  },

  async setStatus(userId: string, isActive: boolean): Promise<AdminUser> {
    const { data } = await apiClient.put<AdminUser>(`/admin/users/${userId}/status`, { isActive });
    return data;
  },

  async setRoles(userId: string, roles: string[]): Promise<AdminUser> {
    const { data } = await apiClient.put<AdminUser>(`/admin/users/${userId}/roles`, { roles });
    return data;
  },

  async verifyEmail(userId: string): Promise<AdminUser> {
    const { data } = await apiClient.post<AdminUser>(`/admin/users/${userId}/verify-email`);
    return data;
  },
};
